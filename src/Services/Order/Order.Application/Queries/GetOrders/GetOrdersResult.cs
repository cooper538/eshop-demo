using Order.Application.Dtos;

namespace Order.Application.Queries.GetOrders;

public sealed record GetOrdersResult(
    IReadOnlyList<OrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
