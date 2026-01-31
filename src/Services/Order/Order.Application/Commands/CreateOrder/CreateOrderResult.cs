namespace EShop.Order.Application.Commands.CreateOrder;

public sealed record CreateOrderResult(Guid OrderId, string Status, string? Message = null);
