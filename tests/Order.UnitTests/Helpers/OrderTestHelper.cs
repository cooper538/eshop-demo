using EShop.Order.Domain.Entities;
using EShop.Order.Domain.Enums;

namespace EShop.Order.UnitTests.Helpers;

public static class OrderTestHelper
{
    public static OrderItem CreateOrderItem(
        Guid? productId = null,
        string productName = "Test Product",
        int quantity = 1,
        decimal unitPrice = 10.00m
    )
    {
        return OrderItem.Create(productId ?? Guid.NewGuid(), productName, quantity, unitPrice);
    }

    public static OrderEntity CreateOrder(
        Guid? customerId = null,
        string customerEmail = "test@example.com",
        IEnumerable<OrderItem>? items = null,
        DateTime? createdAt = null
    )
    {
        return OrderEntity.Create(
            customerId ?? Guid.NewGuid(),
            customerEmail,
            items ?? [CreateOrderItem()],
            createdAt ?? DateTime.UtcNow
        );
    }

    public static OrderEntity CreateConfirmedOrder(
        Guid? customerId = null,
        string customerEmail = "test@example.com",
        IEnumerable<OrderItem>? items = null,
        DateTime? createdAt = null
    )
    {
        var order = CreateOrder(customerId, customerEmail, items, createdAt);
        order.Confirm(DateTime.UtcNow);
        return order;
    }

    public static OrderEntity CreateOrderWithStatus(EOrderStatus targetStatus)
    {
        var order = CreateOrder();

        switch (targetStatus)
        {
            case EOrderStatus.Created:
                return order;

            case EOrderStatus.Confirmed:
                order.Confirm(DateTime.UtcNow);
                break;

            case EOrderStatus.Rejected:
                order.Reject("Test", DateTime.UtcNow);
                break;

            case EOrderStatus.Cancelled:
                order.Confirm(DateTime.UtcNow);
                order.Cancel("Test", DateTime.UtcNow);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus));
        }

        order.ClearDomainEvents();
        return order;
    }
}
