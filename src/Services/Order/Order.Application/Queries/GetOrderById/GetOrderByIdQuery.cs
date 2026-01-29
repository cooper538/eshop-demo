using EShop.Common.Application.Cqrs;
using Order.Application.Dtos;

namespace Order.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;
