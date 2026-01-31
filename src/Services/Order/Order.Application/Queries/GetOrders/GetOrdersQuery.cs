using EShop.Common.Application.Cqrs;

namespace EShop.Order.Application.Queries.GetOrders;

public sealed record GetOrdersQuery(Guid? CustomerId, int Page = 1, int PageSize = 20)
    : IQuery<GetOrdersResult>;
