using EShop.SharedKernel.Events;

namespace EShop.Products.Domain.Events;

public sealed record ProductUpdatedDomainEvent(Guid ProductId, int LowStockThreshold)
    : DomainEventBase;
