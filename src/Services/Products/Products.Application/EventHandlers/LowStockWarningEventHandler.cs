using EShop.Common.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Domain.Events;

namespace Products.Application.EventHandlers;

public sealed class LowStockWarningEventHandler
    : INotificationHandler<DomainEventNotification<LowStockWarningDomainEvent>>
{
    private readonly ILogger<LowStockWarningEventHandler> _logger;

    public LowStockWarningEventHandler(ILogger<LowStockWarningEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<LowStockWarningDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var @event = notification.DomainEvent;

        _logger.LogWarning(
            "Low stock warning for Product {ProductId}: Available {AvailableQuantity}, Threshold {Threshold}",
            @event.ProductId,
            @event.AvailableQuantity,
            @event.Threshold
        );

        // TODO: Publish integration event to Notification service for admin alerts

        return Task.CompletedTask;
    }
}
