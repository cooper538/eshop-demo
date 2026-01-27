using EShop.SharedKernel.Events;

namespace Order.Domain.Events;

public sealed record OrderRejectedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason
) : DomainEventBase;
