using System.Text.Json.Serialization;
using FlowerShop.Api.Data;
using FlowerShop.Api.Models;
using FlowerShop.Api.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("AuthToken", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Auth-Token",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your token here"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "AuthToken"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<CurrentUser>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var currentUser = ctx.RequestServices.GetRequiredService<CurrentUser>();
    var db = ctx.RequestServices.GetRequiredService<AppDbContext>();

    var token = ctx.Request.Headers["X-Auth-Token"].ToString();
    if (!string.IsNullOrWhiteSpace(token))
    {
        var session = await db.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Token == token && s.RevokedAt == null);

        if (session != null)
            currentUser.UserId = session.UserId;
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

// Static Files
app.UseStaticFiles();

// Upload Image
app.MapPost("/upload", async (HttpContext ctx, CurrentUser cu) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();

    var file = ctx.Request.Form.Files.FirstOrDefault();
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowedTypes.Contains(file.ContentType))
        return Results.BadRequest("Only images allowed.");

    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

    using var stream = new FileStream(uploadPath, FileMode.Create);
    await file.CopyToAsync(stream);

    var imageUrl = $"https://localhost:7052/images/{fileName}";
    return Results.Ok(new { imageUrl });
});
// ===== CATEGORIES =====
app.MapGet("/categories", async (AppDbContext db) =>
{
    var list = await db.Categories
        .AsNoTracking()
        .OrderBy(c => c.Name)
        .ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/categories/{id:int}", async (AppDbContext db, int id) =>
{
    var cat = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    return cat is null ? Results.NotFound() : Results.Ok(cat);
});

app.MapPost("/categories", async (AppDbContext db, CurrentUser cu, Category category) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(category.Name))
        return Results.BadRequest("Name is required.");
    category.Name = category.Name.Trim();
    category.CreatedByUserId = cu.UserId;
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/categories/{category.Id}", category);
});

app.MapPut("/categories/{id:int}", async (AppDbContext db, CurrentUser cu, int id, Category input) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var cat = await db.Categories.FindAsync(id);
    if (cat is null) return Results.NotFound();
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name is required.");
    cat.Name = input.Name.Trim();
    cat.Description = input.Description;
    cat.ImageUrl = input.ImageUrl;
    cat.UpdatedByUserId = cu.UserId;
    cat.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(cat);
});

app.MapDelete("/categories/{id:int}", async (AppDbContext db, CurrentUser cu, int id) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var cat = await db.Categories.FindAsync(id);
    if (cat is null) return Results.NotFound();
    db.Categories.Remove(cat);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ===== PRODUCTS =====
app.MapGet("/products", async (
    AppDbContext db,
    string? q,
    int? categoryId,
    decimal? minPrice,
    decimal? maxPrice,
    bool? inStock,
    string? sort) =>
{
    var query = db.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(p =>
            p.Name.Contains(q) ||
            (p.Description ?? "").Contains(q));

    if (categoryId.HasValue)
        query = query.Where(p => p.CategoryId == categoryId.Value);

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice.Value);

    if (inStock == true)
        query = query.Where(p => p.Stock > 0);

    query = sort switch
    {
        "price_asc" => query.OrderBy(p => p.Price),
        "price_desc" => query.OrderByDescending(p => p.Price),
        "name_desc" => query.OrderByDescending(p => p.Name),
        _ => query.OrderBy(p => p.Name)
    };

    var list = await query.ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/products/{id:int}", async (AppDbContext db, int id) =>
{
    var p = await db.Products
        .AsNoTracking()
        .Include(x => x.Category)
        .FirstOrDefaultAsync(x => x.Id == id);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

app.MapPost("/products", async (AppDbContext db, CurrentUser cu, Product product) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(product.Name))
        return Results.BadRequest("Name required.");
    if (product.Price <= 0)
        return Results.BadRequest("Price must be > 0.");
    var categoryExists = await db.Categories.AnyAsync(c => c.Id == product.CategoryId);
    if (!categoryExists)
        return Results.BadRequest("Category not found.");
    product.CreatedByUserId = cu.UserId;
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:int}", async (AppDbContext db, CurrentUser cu, int id, Product input) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name required.");
    if (input.Price <= 0)
        return Results.BadRequest("Price must be > 0.");
    var catExists = await db.Categories.AnyAsync(c => c.Id == input.CategoryId);
    if (!catExists) return Results.BadRequest("Category not found.");
    p.Name = input.Name.Trim();
    p.Description = input.Description;
    p.Price = input.Price;
    p.Stock = input.Stock;
    p.CategoryId = input.CategoryId;
    p.ImageUrl = input.ImageUrl;
    p.UpdatedByUserId = cu.UserId;
    p.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(p);
});

app.MapDelete("/products/{id:int}", async (AppDbContext db, CurrentUser cu, int id) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ===== AUTH =====
app.MapPost("/auth/seed-admin", async (AppDbContext db) =>
{
    if (await db.Users.AnyAsync())
        return Results.BadRequest("Users already exist.");
    var (hash, salt) = PasswordHasher.HashPassword("Admin123!");
    var admin = new User
    {
        FullName = "Admin",
        Email = "admin@flowershop.local",
        PasswordHash = hash,
        PasswordSalt = salt,
        IsAdmin = true
    };
    db.Users.Add(admin);
    await db.SaveChangesAsync();
    return Results.Ok(new { admin.Email, Password = "Admin123!" });
});

app.MapPost("/auth/register", async (AppDbContext db, RegisterRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.FullName)) return Results.BadRequest("FullName required.");
    if (string.IsNullOrWhiteSpace(req.Email)) return Results.BadRequest("Email required.");
    if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6) return Results.BadRequest("Password min 6.");
    var email = req.Email.Trim().ToLowerInvariant();
    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists) return Results.BadRequest("Email already used.");
    var (hash, salt) = PasswordHasher.HashPassword(req.Password);
    var user = new User
    {
        FullName = req.FullName.Trim(),
        Email = email,
        PasswordHash = hash,
        PasswordSalt = salt
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", new { user.Id, user.FullName, user.Email });
});

app.MapPost("/auth/login", async (AppDbContext db, LoginRequest req) =>
{
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    if (user == null) return Results.Unauthorized();
    if (!PasswordHasher.Verify(req.Password, user.PasswordHash, user.PasswordSalt))
        return Results.Unauthorized();
    var token = Guid.NewGuid().ToString();
    db.UserSessions.Add(new UserSession { UserId = user.Id, Token = token });
    await db.SaveChangesAsync();
    return Results.Ok(new LoginResponse(token, user.Id, user.FullName, user.Email, user.IsAdmin));
});

app.MapPost("/auth/logout", async (AppDbContext db, CurrentUser cu, HttpContext ctx) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var token = ctx.Request.Headers["X-Auth-Token"].ToString();
    var session = await db.UserSessions.FirstOrDefaultAsync(s => s.Token == token && s.RevokedAt == null);
    if (session == null) return Results.BadRequest("Invalid session.");
    session.RevokedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/auth/change-password", async (AppDbContext db, CurrentUser cu, ChangePasswordRequest req) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var user = await db.Users.FirstAsync(u => u.Id == cu.UserId);
    if (!PasswordHasher.Verify(req.OldPassword, user.PasswordHash, user.PasswordSalt))
        return Results.BadRequest("Old password wrong.");
    if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
        return Results.BadRequest("New password min 6.");
    var (hash, salt) = PasswordHasher.HashPassword(req.NewPassword);
    user.PasswordHash = hash;
    user.PasswordSalt = salt;
    user.UpdatedByUserId = cu.UserId;
    user.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.MapPost("/auth/forgot-password", async (AppDbContext db, ForgotPasswordRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest("Email zorunludur.");

    if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
        return Results.BadRequest("Yeni şifre en az 6 karakter olmalıdır.");

    if (req.NewPassword != req.ConfirmPassword)
        return Results.BadRequest("Şifreler eşleşmiyor.");

    var email = req.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    if (user == null)
        return Results.BadRequest("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

    var (hash, salt) = PasswordHasher.HashPassword(req.NewPassword);
    user.PasswordHash = hash;
    user.PasswordSalt = salt;
    user.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok("Şifre başarıyla güncellendi.");
});
// ===== ORDERS =====
app.MapPost("/orders", async (AppDbContext db, FlowerShop.Api.Security.CurrentUser cu, CreateOrderRequest req) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    if (req.Items is null || req.Items.Count == 0)
        return Results.BadRequest("Items required.");
    if (req.Items.Any(i => i.Quantity <= 0))
        return Results.BadRequest("Quantity must be > 0.");

    var allowedPayments = new[] { "Cash", "Card", "Online" };
    if (!allowedPayments.Contains(req.PaymentMethod))
        return Results.BadRequest("Invalid payment method.");

    var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
    var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
    if (products.Count != productIds.Count)
        return Results.BadRequest("One or more products not found.");

    var order = new Order
    {
        UserId = cu.UserId!.Value,
        DeliveryDate = req.DeliveryDate,
        DeliveryTime = req.DeliveryTime,
        IsSurprise = req.IsSurprise,
        Status = "Pending",
        PaymentMethod = req.PaymentMethod,
        PaymentStatus = req.PaymentMethod == "Online" ? "Paid" : "Pending",
        CreatedByUserId = cu.UserId
    };

    foreach (var item in req.Items)
    {
        var p = products.First(x => x.Id == item.ProductId);
        order.Items.Add(new OrderItem
        {
            ProductId = p.Id,
            Quantity = item.Quantity,
            UnitPrice = p.Price,
            CreatedByUserId = cu.UserId
        });
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", new { order.Id });
});

app.MapGet("/orders/my", async (AppDbContext db, FlowerShop.Api.Security.CurrentUser cu) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var list = await db.Orders
        .AsNoTracking()
        .Include(o => o.Items).ThenInclude(i => i.Product)
        .Where(o => o.UserId == cu.UserId)
        .OrderByDescending(o => o.Id)
        .Select(o => new
        {
            o.Id,
            o.DeliveryDate,
            o.IsSurprise,
            o.Status,
            o.PaymentMethod,
            o.PaymentStatus,
            TotalPrice = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            ProductName = o.Items.Select(i => i.Product!.Name).FirstOrDefault() ?? ""
        })
        .ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/orders/{id:int}", async (AppDbContext db, FlowerShop.Api.Security.CurrentUser cu, int id) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var order = await db.Orders
        .AsNoTracking()
        .Where(o => o.Id == id && o.UserId == cu.UserId)
        .Select(o => new
        {
            o.Id,
            o.DeliveryDate,
            o.DeliveryTime,
            o.IsSurprise,
            o.Status,
            o.PaymentMethod,
            o.PaymentStatus,
            Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = o.Items.Select(i => new { i.ProductId, i.Quantity, i.UnitPrice })
        })
        .FirstOrDefaultAsync();
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPut("/orders/{id:int}/status", async (
    AppDbContext db,
    FlowerShop.Api.Security.CurrentUser cu,
    int id,
    UpdateOrderStatusRequest req) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var allowedStatuses = new[] { "Pending", "Confirmed", "Preparing", "OnTheWay", "Delivered", "Cancelled" };
    if (!allowedStatuses.Contains(req.Status))
        return Results.BadRequest("Invalid status.");
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();
    order.Status = req.Status;
    order.UpdatedByUserId = cu.UserId;
    order.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { order.Id, order.Status });
});

app.MapGet("/orders", async (AppDbContext db, FlowerShop.Api.Security.CurrentUser cu) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var list = await db.Orders
        .AsNoTracking()
        .Include(o => o.Items).ThenInclude(i => i.Product)
        .Include(o => o.User)
        .OrderByDescending(o => o.Id)
        .Select(o => new
        {
            o.Id,
            o.DeliveryDate,
            o.IsSurprise,
            o.Status,
            o.PaymentMethod,
            o.PaymentStatus,
            UserName = o.User!.FullName,
            TotalPrice = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            ProductName = o.Items.Select(i => i.Product!.Name).FirstOrDefault() ?? ""
        })
        .ToListAsync();
    return Results.Ok(list);
});

// ===== ADMIN =====
app.MapGet("/admin/stats", async (AppDbContext db, CurrentUser cu) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var stats = new
    {
        TotalOrders = await db.Orders.CountAsync(),
        PendingOrders = await db.Orders.CountAsync(o => o.Status == "Pending"),
        TotalProducts = await db.Products.CountAsync(),
        TotalUsers = await db.Users.CountAsync(),
        TotalRevenue = await db.OrderItems.SumAsync(i => i.UnitPrice * i.Quantity)
    };
    return Results.Ok(stats);
});

app.MapGet("/admin/users", async (AppDbContext db, CurrentUser cu) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();
    var users = await db.Users
        .AsNoTracking()
        .OrderBy(u => u.FullName)
        .Select(u => new { u.Id, u.FullName, u.Email, u.IsActive, u.IsAdmin })
        .ToListAsync();
    return Results.Ok(users);
});

app.MapGet("/", () => "FlowerShop API is running ✅");
app.Run();