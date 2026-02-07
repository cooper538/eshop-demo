using EShop.Common.Application.Events;
using EShop.Common.Application.Exceptions;
using EShop.Contracts.IntegrationEvents.Product;
using EShop.Products.Application.Data;
using EShop.Products.Domain.Entities;
using EShop.Products.Domain.Events;
using EShop.SharedKernel.Services;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Products.Application.EventHandlers;

public sealed class ProductUpdatedEventHandler(
    IProductDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProductUpdatedEventHandler> logger
) : INotificationHandler<DomainEventNotification<ProductUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var @event = notification.DomainEvent;

        var stock = await dbContext.Stocks.FirstOrDefaultAsync(
            s => s.ProductId == @event.ProductId,
            cancellationToken
        );

        if (stock is null)
        {
            throw NotFoundException.For<StockEntity>(@event.ProductId);
        }

        stock.UpdateLowStockThreshold(@event.LowStockThreshold);

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
