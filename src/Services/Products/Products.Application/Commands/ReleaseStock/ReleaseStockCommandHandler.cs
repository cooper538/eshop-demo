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
        // Find all stocks with active reservations for this order
        var stocks = await _dbContext
            .Stocks.Include(s =>
                s.Reservations.Where(r =>
                    r.OrderId == request.OrderId && r.Status == EReservationStatus.Active
                )
            )
            .Where(s =>
                s.Reservations.Any(r =>
                    r.OrderId == request.OrderId && r.Status == EReservationStatus.Active
                )
            )
            .ToListAsync(cancellationToken);

        // Idempotent: if no active reservations, consider it already released
        if (stocks.Count == 0)
        {
            return StockReleaseResult.Succeeded();
        }

        // Release reservations through the stock aggregate
        foreach (var stock in stocks)
        {
            stock.ReleaseReservation(request.OrderId);
        }

        return StockReleaseResult.Succeeded();
    }
}
