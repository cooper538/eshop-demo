using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record StockReservationExpiredDomainEvent(Guid OrderId, Guid ProductId, int Quantity)
    : DomainEventBase;
