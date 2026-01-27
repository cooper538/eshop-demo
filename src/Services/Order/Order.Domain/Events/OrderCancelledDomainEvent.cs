using EShop.SharedKernel.Events;

namespace Order.Domain.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId, Guid CustomerId, string Reason)
    : DomainEventBase;
