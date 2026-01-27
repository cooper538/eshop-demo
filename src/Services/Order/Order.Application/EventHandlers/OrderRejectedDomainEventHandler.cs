using EShop.Common.Events;
using EShop.Contracts.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderRejectedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<OrderRejectedDomainEventHandler> logger
) : INotificationHandler<DomainEventNotification<OrderRejectedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderRejectedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        logger.LogInformation(
            "Publishing OrderRejectedEvent for Order {OrderId}, Customer {CustomerId}, Reason: {Reason}",
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.Reason
        );

        var integrationEvent = new OrderRejectedEvent(
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.CustomerEmail,
            domainEvent.Reason
        );

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
