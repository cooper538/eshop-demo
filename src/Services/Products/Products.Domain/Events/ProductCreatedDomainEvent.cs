using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    int InitialStockQuantity,
    int LowStockThreshold
) : DomainEventBase;
