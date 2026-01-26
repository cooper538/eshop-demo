namespace EShop.Contracts.ServiceClients.Product;

public sealed record ReserveStockRequest(Guid OrderId, IReadOnlyList<OrderItemRequest> Items);

public sealed record OrderItemRequest(Guid ProductId, int Quantity);
