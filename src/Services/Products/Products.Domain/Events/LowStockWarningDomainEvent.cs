using EShop.SharedKernel.Events;

namespace Products.Domain.Events;

public sealed record LowStockWarningDomainEvent(
    Guid ProductId,
    int AvailableQuantity,
    int Threshold
) : DomainEventBase;
