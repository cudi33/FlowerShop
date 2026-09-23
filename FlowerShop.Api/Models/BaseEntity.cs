namespace FlowerShop.Api.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}