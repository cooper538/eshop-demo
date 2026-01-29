namespace EShop.Contracts.IntegrationEvents.Order;

public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason
) : IntegrationEvent;
