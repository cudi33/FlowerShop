namespace FlowerShop.Maui.Models;

public class OrderResponse
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public bool IsSurprise { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}