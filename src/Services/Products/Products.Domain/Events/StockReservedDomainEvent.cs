using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record StockReservedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    DateTime ReservedAt
) : DomainEventBase;
