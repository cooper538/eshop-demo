namespace EShop.Contracts.Dtos.Order;

public sealed record OrderSummaryDto(
    Guid OrderId,
    Guid CustomerId,
    DateTime CreatedAt,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items
);
