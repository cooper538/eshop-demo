using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Data;
using EShop.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderDbContext _dbContext;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderDbContext dbContext,
        IProductServiceClient productServiceClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<CreateOrderCommandHandler> logger
    )
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = request.ToEntity(_dateTimeProvider.UtcNow);

        var orderItems = request
            .Items.Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
            .ToList();

        var reservationRequest = new ReserveStockRequest(order.Id, orderItems);

        StockReservationResult reservationResult;
        try
        {
            reservationResult = await _productServiceClient.ReserveStockAsync(
                reservationRequest,
                cancellationToken
            );
        }
        catch (ServiceClientException ex)
        {
            // Technical failure (service unavailable, timeout) - save as Created for retry
            _logger.LogWarning(ex, "Stock reservation failed for order {OrderId}", order.Id);
            _dbContext.Orders.Add(order);
            return new CreateOrderResult(
                order.Id,
                order.Status.ToString(),
                "Stock reservation pending"
            );
        }

        if (!reservationResult.Success)
        {
            // Validation error (product not found) - throw validation exception for 400 response
            if (reservationResult.ErrorCode == EStockReservationErrorCode.ProductNotFound)
            {
                throw new ValidationException(
                    reservationResult.FailureReason ?? "One or more products not found"
                );
            }

            // Business rejection (insufficient stock) - reject order
            order.Reject(
                reservationResult.FailureReason ?? "Stock reservation failed",
                _dateTimeProvider.UtcNow
            );
            _dbContext.Orders.Add(order);

            return new CreateOrderResult(
                order.Id,
                order.Status.ToString(),
                reservationResult.FailureReason
            );
        }

        order.Confirm(_dateTimeProvider.UtcNow);
        _dbContext.Orders.Add(order);

        return new CreateOrderResult(order.Id, order.Status.ToString());
    }
}
