using EShop.Common.Application.Cqrs;
using EShop.Order.Application.Dtos;

namespace EShop.Order.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;
