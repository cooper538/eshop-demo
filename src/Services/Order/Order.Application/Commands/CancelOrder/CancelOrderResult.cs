namespace Order.Application.Commands.CancelOrder;

public sealed record CancelOrderResult(
    Guid OrderId,
    string Status,
    bool Success,
    string? Message = null
);
