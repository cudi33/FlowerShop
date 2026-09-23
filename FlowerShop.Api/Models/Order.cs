namespace FlowerShop.Api.Models;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public TimeOnly DeliveryTime { get; set; }
    public bool IsSurprise { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Preparing, OnTheWay, Delivered, Cancelled
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Online
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid
    public List<OrderItem> Items { get; set; } = new();
}