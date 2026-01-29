using EShop.Common.Application.Events;
using EShop.Contracts.IntegrationEvents.Order;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderConfirmedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
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
        )
        {
            Timestamp = dateTimeProvider.UtcNow,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
