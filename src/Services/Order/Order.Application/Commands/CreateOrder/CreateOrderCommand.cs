using EShop.Common.Application.Cqrs;

namespace EShop.Order.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<CreateOrderItemDto> Items
) : ICommand<CreateOrderResult>;

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);
