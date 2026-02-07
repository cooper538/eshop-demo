using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Data;
using EShop.Order.Domain.Entities;
using EShop.SharedKernel.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var productSnapshots = await _dbContext
            .ProductSnapshots.Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        if (productSnapshots.Count != productIds.Count)
        {
            var missingIds = productIds.Except(productSnapshots.Keys);
            throw new ValidationException($"Products not found: {string.Join(", ", missingIds)}");
        }

        var orderItems = request.Items.Select(i =>
        {
            var snapshot = productSnapshots[i.ProductId];
            return OrderItem.Create(i.ProductId, snapshot.Name, i.Quantity, snapshot.Price);
        });

        var order = OrderEntity.Create(
            request.CustomerId,
            request.CustomerEmail,
            orderItems,
            _dateTimeProvider.UtcNow
        );

        var stockItems = request
            .Items.Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
            .ToList();

        var reservationRequest = new ReserveStockRequest(order.Id, stockItems);

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
            if (reservationResult.ErrorCode == EStockReservationErrorCode.ProductNotFound)
            {
                throw new ValidationException(
                    reservationResult.FailureReason ?? "One or more products not found"
                );
            }

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
