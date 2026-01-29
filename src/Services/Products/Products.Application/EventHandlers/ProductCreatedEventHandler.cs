using EShop.Common.Application.Events;
using MediatR;
using Products.Application.Data;
using Products.Domain.Entities;
using Products.Domain.Events;

namespace Products.Application.EventHandlers;

public sealed class ProductCreatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductCreatedDomainEvent>>
{
    private readonly IProductDbContext _dbContext;

    public ProductCreatedEventHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Handle(
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

        _dbContext.Stocks.Add(stock);

        return Task.CompletedTask;
    }
}
