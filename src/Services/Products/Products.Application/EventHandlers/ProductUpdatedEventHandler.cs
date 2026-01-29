using EShop.Common.Application.Events;
using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Domain.Entities;
using Products.Domain.Events;

namespace Products.Application.EventHandlers;

public sealed class ProductUpdatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductUpdatedDomainEvent>>
{
    private readonly IProductDbContext _dbContext;

    public ProductUpdatedEventHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        DomainEventNotification<ProductUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var @event = notification.DomainEvent;

        var stock = await _dbContext.Stocks.FirstOrDefaultAsync(
            s => s.ProductId == @event.ProductId,
            cancellationToken
        );

        if (stock is null)
        {
            throw NotFoundException.For<StockEntity>(@event.ProductId);
        }

        stock.UpdateLowStockThreshold(@event.LowStockThreshold);
    }
}
