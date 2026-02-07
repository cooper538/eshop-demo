using EShop.Common.Application.Events;
using EShop.Contracts.IntegrationEvents.Product;
using EShop.Products.Application.Data;
using EShop.Products.Domain.Entities;
using EShop.Products.Domain.Events;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EShop.Products.Application.EventHandlers;

public sealed class ProductCreatedEventHandler(
    IProductDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProductCreatedEventHandler> logger
) : INotificationHandler<DomainEventNotification<ProductCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var @event = notification.DomainEvent;

        var stock = StockEntity.Create(
            @event.ProductId,
            @event.InitialStockQuantity,
            @event.LowStockThreshold
        );

        dbContext.Stocks.Add(stock);

        var product = await dbContext.Products.FindAsync([@event.ProductId], cancellationToken);

        if (product is null)
        {
            logger.LogError(
                "Cannot publish ProductChangedEvent - Product {ProductId} not found",
                @event.ProductId
            );
            return;
        }

        var integrationEvent = new ProductChangedEvent(product.Id, product.Name, product.Price)
        {
            Timestamp = dateTimeProvider.UtcNow,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
