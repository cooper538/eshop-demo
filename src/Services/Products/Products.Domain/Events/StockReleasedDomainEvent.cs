using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record StockReleasedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    DateTime ReleasedAt
) : DomainEventBase;
