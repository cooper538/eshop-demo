using EShop.Common.Application.Exceptions;
using EShop.Contracts.ServiceClients;
using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Data;
using EShop.Order.Domain.Entities;
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
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        GetProductsResult productsResult;
        try
        {
            productsResult = await _productServiceClient.GetProductsAsync(
                productIds,
                cancellationToken
            );
        }
        catch (ServiceClientException ex) when (ex.ErrorCode == EServiceClientErrorCode.NotFound)
        {
            throw new ValidationException("One or more products not found");
        }

        var productLookup = productsResult.Products.ToDictionary(p => p.ProductId);

        var orderItems = request.Items.Select(i =>
        {
            if (!productLookup.TryGetValue(i.ProductId, out var product))
            {
                throw new ValidationException($"Product {i.ProductId} not found");
            }
            return OrderItem.Create(i.ProductId, product.Name, i.Quantity, product.Price);
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
