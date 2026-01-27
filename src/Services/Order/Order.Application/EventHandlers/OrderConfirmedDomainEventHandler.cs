using EShop.Common.Events;
using EShop.Contracts.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderConfirmedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<OrderConfirmedDomainEventHandler> logger
) : INotificationHandler<DomainEventNotification<OrderConfirmedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderConfirmedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        logger.LogInformation(
            "Publishing OrderConfirmedEvent for Order {OrderId}, Customer {CustomerId}",
            domainEvent.OrderId,
            domainEvent.CustomerId
        );

        var integrationEvent = new OrderConfirmedEvent(
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.TotalAmount,
            domainEvent.CustomerEmail
        );

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
