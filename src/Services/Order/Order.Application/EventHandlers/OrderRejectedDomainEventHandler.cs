using EShop.Common.Application.Events;
using EShop.Contracts.IntegrationEvents.Order;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderRejectedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
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
        )
        {
            Timestamp = dateTimeProvider.UtcNow,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
