using EShop.Common.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Domain.Events;

namespace Products.Application.EventHandlers;

public sealed class StockReservationExpiredEventHandler
    : INotificationHandler<DomainEventNotification<StockReservationExpiredDomainEvent>>
{
    private readonly ILogger<StockReservationExpiredEventHandler> _logger;

    public StockReservationExpiredEventHandler(ILogger<StockReservationExpiredEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<StockReservationExpiredDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var @event = notification.DomainEvent;

        _logger.LogInformation(
            "Stock reservation expired for Order {OrderId}, Product {ProductId}, Quantity {Quantity}",
            @event.OrderId,
            @event.ProductId,
            @event.Quantity
        );

        // TODO: Publish integration event to Order service to cancel the order

        return Task.CompletedTask;
    }
}
