using EShop.Common.Application.Cqrs;

namespace Order.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand<CancelOrderResult>;
