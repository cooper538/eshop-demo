using EShop.Common.Application.Cqrs;
using EShop.Order.Domain.Entities;

namespace EShop.Order.Application.Commands.CreateOrder;

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
    public static OrderEntity ToEntity(this CreateOrderCommand command, DateTime createdAt)
    {
        var items = command.Items.Select(i =>
            OrderItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)
        );

        return OrderEntity.Create(command.CustomerId, command.CustomerEmail, items, createdAt);
    }
}
