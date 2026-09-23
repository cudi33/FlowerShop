namespace FlowerShop.Maui.Models;

public class OrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateOnly DeliveryDate { get; set; }
    public TimeOnly DeliveryTime { get; set; }
    public bool IsSurprise { get; set; }
    public string? Notes { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}