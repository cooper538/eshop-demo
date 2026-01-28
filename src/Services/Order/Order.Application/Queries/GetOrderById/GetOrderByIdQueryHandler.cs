using EShop.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Data;
using Order.Application.Dtos;
using Order.Domain.Entities;

namespace Order.Application.Queries.GetOrderById;

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
