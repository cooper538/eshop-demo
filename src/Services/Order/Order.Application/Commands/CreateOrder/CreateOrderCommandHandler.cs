using EShop.Contracts.ServiceClients.Product;
using MediatR;
using Order.Application.Data;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderDbContext _dbContext;
    private readonly IProductServiceClient _productServiceClient;

    public CreateOrderCommandHandler(
        IOrderDbContext dbContext,
        IProductServiceClient productServiceClient
    )
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = request.ToEntity();

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

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id, order.Status.ToString());
    }
}
