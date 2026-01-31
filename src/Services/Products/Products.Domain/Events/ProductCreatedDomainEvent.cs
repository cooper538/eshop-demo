using EShop.SharedKernel.Events;

namespace EShop.Products.Domain.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    int InitialStockQuantity,
    int LowStockThreshold
) : DomainEventBase;
