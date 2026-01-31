using EShop.Common.Application.Events;
using EShop.Contracts.IntegrationEvents.Product;
using EShop.Products.Application.Data;
using EShop.Products.Domain.Events;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Products.Application.EventHandlers;

public sealed class LowStockWarningEventHandler(
    IProductDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
    ILogger<LowStockWarningEventHandler> logger
) : INotificationHandler<DomainEventNotification<LowStockWarningDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LowStockWarningDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        logger.LogWarning(
            "Low stock warning for Product {ProductId}: Available {AvailableQuantity}, Threshold {Threshold}",
            domainEvent.ProductId,
            domainEvent.AvailableQuantity,
            domainEvent.Threshold
        );

        var productName = await dbContext
            .Products.Where(p => p.Id == domainEvent.ProductId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (productName is null)
        {
            logger.LogError(
                "Cannot publish StockLowEvent - Product {ProductId} not found",
                domainEvent.ProductId
            );
            return;
        }

        logger.LogInformation(
            "Publishing StockLowEvent for Product {ProductId} ({ProductName})",
            domainEvent.ProductId,
            productName
        );

        var integrationEvent = new StockLowEvent(
            domainEvent.ProductId,
            productName,
            domainEvent.AvailableQuantity,
            domainEvent.Threshold
        )
        {
            Timestamp = dateTimeProvider.UtcNow,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
