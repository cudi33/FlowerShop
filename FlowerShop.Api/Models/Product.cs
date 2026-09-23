namespace FlowerShop.Api.Models;

public class Product : BaseEntity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? ImageUrl { get; set; } // أضيف هاد
    public List<OrderItem> Items { get; set; } = new();
}