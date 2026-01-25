using EShop.Common.Cqrs;
using Order.Domain.Entities;

namespace Order.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<CreateOrderItemDto> Items
) : ICommand<CreateOrderResult>;

public sealed record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public static class CreateOrderCommandMapper
{
    public static OrderEntity ToEntity(this CreateOrderCommand command)
    {
        var items = command.Items.Select(i =>
            OrderItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)
        );

        return OrderEntity.Create(command.CustomerId, command.CustomerEmail, items);
    }
}
