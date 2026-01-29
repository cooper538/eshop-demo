namespace EShop.Contracts.IntegrationEvents.Order;

public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string CustomerEmail
) : IntegrationEvent;
