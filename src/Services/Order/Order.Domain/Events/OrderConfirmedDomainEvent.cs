using EShop.SharedKernel.Events;

namespace EShop.Order.Domain.Events;

public sealed record OrderConfirmedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    IReadOnlyList<OrderItemInfo> Items
) : DomainEventBase;
