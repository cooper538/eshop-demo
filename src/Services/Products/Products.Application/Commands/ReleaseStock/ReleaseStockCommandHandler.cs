using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;
using Products.Domain.Enums;

namespace Products.Application.Commands.ReleaseStock;

public sealed class ReleaseStockCommandHandler
    : IRequestHandler<ReleaseStockCommand, StockReleaseResult>
{
    private readonly IProductDbContext _dbContext;

    public ReleaseStockCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockReleaseResult> Handle(
        ReleaseStockCommand request,
        CancellationToken cancellationToken
    )
    {
        // Find all active reservations for this order
        var reservations = await _dbContext
            .StockReservations.Where(r => r.OrderId == request.OrderId)
            .Where(r => r.Status == EReservationStatus.Active)
            .ToListAsync(cancellationToken);

        // Idempotent: if no active reservations, consider it already released
        if (reservations.Count == 0)
        {
            return StockReleaseResult.Succeeded();
        }

        // Get products to release stock back
        var productIds = reservations.Select(r => r.ProductId).ToList();
        var products = await _dbContext
            .Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Release stock and mark reservations as released
        foreach (var reservation in reservations)
        {
            var product = products[reservation.ProductId];
            product.ReleaseStock(reservation.Quantity);
            reservation.Release();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return StockReleaseResult.Succeeded();
    }
}
