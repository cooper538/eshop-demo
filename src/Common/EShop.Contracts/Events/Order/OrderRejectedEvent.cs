namespace EShop.Contracts.Events.Order;

public sealed record OrderRejectedEvent(Guid OrderId, Guid CustomerId, string Reason) : IntegrationEvent;
