using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Application.Data;
using EShop.SharedKernel.Services;
using MediatR;

namespace EShop.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderDbContext _dbContext;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateOrderCommandHandler(
        IOrderDbContext dbContext,
        IProductServiceClient productServiceClient,
        IDateTimeProvider dateTimeProvider
    )
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
        _dateTimeProvider = dateTimeProvider;
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

        var reservationResult = await _productServiceClient.ReserveStockAsync(
            reservationRequest,
            cancellationToken
        );

        if (!reservationResult.Success)
        {
            throw new InvalidOperationException(
                $"Stock reservation failed: {reservationResult.FailureReason}"
            );
        }

        order.Confirm(_dateTimeProvider.UtcNow);
        _dbContext.Orders.Add(order);

        return new CreateOrderResult(order.Id, order.Status.ToString());
    }
}
