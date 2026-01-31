using EShop.Order.Application.Dtos;

namespace EShop.Order.Application.Queries.GetOrders;

public sealed record GetOrdersResult(
    IReadOnlyList<OrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
