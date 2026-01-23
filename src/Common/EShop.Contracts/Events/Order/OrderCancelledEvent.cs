namespace EShop.Contracts.Events.Order;

public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : IntegrationEvent;
