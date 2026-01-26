using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record ProductUpdatedDomainEvent(Guid ProductId, int LowStockThreshold)
    : DomainEventBase;
