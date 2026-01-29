namespace EShop.Contracts.IntegrationEvents.Order;

public sealed record OrderRejectedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason
) : IntegrationEvent;
