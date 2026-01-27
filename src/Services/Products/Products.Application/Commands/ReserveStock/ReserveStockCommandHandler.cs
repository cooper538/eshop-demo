using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;
using Products.Domain.Entities;
using Products.Domain.Enums;

namespace Products.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand, StockReservationResult>
{
    private readonly IProductDbContext _dbContext;

    public ReserveStockCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockReservationResult> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken
    )
    {
        var idempotencyResult = await CheckIdempotencyAsync(request.OrderId, cancellationToken);
        if (idempotencyResult != null)
        {
            return idempotencyResult;
        }

        var stocks = await LoadStocksAsync(request.Items, cancellationToken);

        foreach (var item in request.Items)
        {
            stocks[item.ProductId].ReserveStock(request.OrderId, item.Quantity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return StockReservationResult.Succeeded();
    }

    private async Task<StockReservationResult?> CheckIdempotencyAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        var existing = await _dbContext
            .Stocks.SelectMany(s => s.Reservations)
            .Where(r => r.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing == null)
        {
            return null;
        }

        return existing.Status == EReservationStatus.Active
            ? StockReservationResult.Succeeded()
            : StockReservationResult.AlreadyProcessed(existing.Status.ToString());
    }

    private async Task<Dictionary<Guid, StockEntity>> LoadStocksAsync(
        IReadOnlyList<OrderItemDto> items,
        CancellationToken cancellationToken
    )
    {
        var productIds = items.Select(i => i.ProductId).ToList();

        return await _dbContext
            .Stocks.Include(s => s.Reservations.Where(r => r.Status == EReservationStatus.Active))
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, cancellationToken);
    }
}
