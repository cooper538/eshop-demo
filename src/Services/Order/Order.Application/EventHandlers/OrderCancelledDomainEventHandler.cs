using EShop.Common.Events;
using EShop.Contracts.Events.Order;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderCancelledDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
    ILogger<OrderCancelledDomainEventHandler> logger
) : INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCancelledDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        logger.LogInformation(
            "Publishing OrderCancelledEvent for Order {OrderId}, Customer {CustomerId}, Reason: {Reason}",
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.Reason
        );

        var integrationEvent = new OrderCancelledEvent(
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.CustomerEmail,
            domainEvent.Reason
        )
        {
            Timestamp = dateTimeProvider.UtcNow,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
