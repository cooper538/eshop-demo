namespace EShop.E2E.Tests.Fixtures;

public sealed record CreateOrderResponse(Guid OrderId, string Status, string? Message);

public sealed record CancelOrderResponse(Guid OrderId, string Status, bool Success);

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderItemDto> Items
);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public sealed record GetOrdersResponse(
    List<OrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
