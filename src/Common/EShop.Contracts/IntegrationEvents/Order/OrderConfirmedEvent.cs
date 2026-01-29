namespace EShop.Contracts.Events.Order;

public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string CustomerEmail
) : IntegrationEvent;
