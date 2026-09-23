using CommunityToolkit.Maui;
using FlowerShop.Maui.Pages;
using FlowerShop.Maui.Services;
using Microsoft.Extensions.Logging;

namespace FlowerShop.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7052")
        });

        // Services
        builder.Services.AddSingleton<IProductService, ProductService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddTransient<IOrderService, OrderService>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<RegisterPage>();
        builder.Services.AddSingleton<ProductsPage>();
        builder.Services.AddTransient<OrderPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<ChangePasswordPage>();
        builder.Services.AddTransient<AdminOrdersPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<AdminCategoriesPage>();
        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<AdminUsersPage>();
        builder.Services.AddTransient<AdminProductsPage>();
        builder.Services.AddTransient<PaymentPage>();
        builder.Services.AddTransient<StatusPopup>();
        // Shell
        builder.Services.AddSingleton<AppShell>();

        // Routes
        Routing.RegisterRoute("myorders", typeof(OrdersPage));
        Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}