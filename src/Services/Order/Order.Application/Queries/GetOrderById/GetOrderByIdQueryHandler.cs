using EShop.Common.Application.Exceptions;
using EShop.Order.Application.Data;
using EShop.Order.Application.Dtos;
using EShop.Order.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderDbContext _dbContext;

    public GetOrderByIdQueryHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var order = await _dbContext
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
        {
            throw NotFoundException.For<OrderEntity>(request.Id);
        }

        return OrderDto.FromEntity(order);
    }
}
