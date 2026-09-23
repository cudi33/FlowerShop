namespace FlowerShop.Api.Models;

public class Category : BaseEntity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; } // أضيف هاد
    public List<Product> Products { get; set; } = new();
}