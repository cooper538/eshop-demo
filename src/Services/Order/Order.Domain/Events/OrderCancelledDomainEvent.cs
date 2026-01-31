using EShop.SharedKernel.Events;

namespace EShop.Order.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason
) : DomainEventBase;
