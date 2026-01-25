using MediatR;
using Order.Application.Data;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderDbContext _dbContext;

    public CreateOrderCommandHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = request.ToEntity();

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id, order.Status.ToString());
    }
}
