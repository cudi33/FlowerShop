namespace FlowerShop.Api.Models;

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record UpdateOrderStatusRequest(string Status);

public record CreateOrderRequest(
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    bool IsSurprise,
    List<CreateOrderItemRequest> Items,
    string PaymentMethod  // Cash, Card, Online
);