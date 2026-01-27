namespace Order.Domain.Events;

public sealed record OrderItemInfo(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
