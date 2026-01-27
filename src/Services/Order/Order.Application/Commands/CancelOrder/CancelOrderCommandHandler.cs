using EShop.Common.Exceptions;
using EShop.Contracts.ServiceClients.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Data;
using Order.Domain.Entities;
using Order.Domain.Exceptions;

namespace Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderDbContext _dbContext;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderDbContext dbContext,
        IProductServiceClient productServiceClient,
        ILogger<CancelOrderCommandHandler> logger
    )
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
        _logger = logger;
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

        // Release reserved stock - don't fail cancellation if release fails
        await ReleaseStockAsync(order.Id, cancellationToken);

        return new CancelOrderResult(order.Id, order.Status.ToString(), Success: true);
    }

    private async Task ReleaseStockAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var releaseRequest = new ReleaseStockRequest(orderId);
            var result = await _productServiceClient.ReleaseStockAsync(
                releaseRequest,
                cancellationToken
            );

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Failed to release stock for order {OrderId}: {Reason}",
                    orderId,
                    result.FailureReason
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing stock for order {OrderId}", orderId);
        }
    }
}
