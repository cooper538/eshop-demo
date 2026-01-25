using EShop.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Data;
using Order.Domain.Entities;
using Order.Domain.Exceptions;

namespace Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderDbContext _dbContext;

    public CancelOrderCommandHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CancelOrderResult> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId,
            cancellationToken
        );

        if (order is null)
        {
            throw NotFoundException.For<OrderEntity>(request.OrderId);
        }

        try
        {
            order.Cancel(request.Reason);
        }
        catch (InvalidOrderStateException ex)
        {
            return new CancelOrderResult(
                order.Id,
                order.Status.ToString(),
                Success: false,
                Message: ex.Message
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelOrderResult(order.Id, order.Status.ToString(), Success: true);
    }
}
