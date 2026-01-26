using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.Data;
using Products.Domain.Enums;

namespace Products.Application.Commands.ExpireReservations;

public sealed partial class ExpireReservationsCommandHandler
    : IRequestHandler<ExpireReservationsCommand>
{
    private readonly IProductDbContext _dbContext;
    private readonly ILogger<ExpireReservationsCommandHandler> _logger;

    public ExpireReservationsCommandHandler(
        IProductDbContext dbContext,
        ILogger<ExpireReservationsCommandHandler> logger
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Find stocks with expired reservations
        var stocks = await _dbContext
            .Stocks.Include(s =>
                s.Reservations.Where(r =>
                    r.Status == EReservationStatus.Active && r.ExpiresAt < now
                )
            )
            .Where(s =>
                s.Reservations.Any(r => r.Status == EReservationStatus.Active && r.ExpiresAt < now)
            )
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (stocks.Count == 0)
        {
            return;
        }

        var totalExpired = 0;

        foreach (var stock in stocks)
        {
            var expiredReservations = stock.Reservations.ToList();

            foreach (var reservation in expiredReservations)
            {
                stock.ExpireReservation(reservation.Id);
                LogReservationExpired(
                    reservation.OrderId,
                    reservation.ProductId,
                    reservation.Quantity
                );
                totalExpired++;
            }
        }

        LogFoundExpiredReservations(totalExpired);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            LogProcessingCompleted(totalExpired);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Another instance processed some reservations - will retry on next run
            LogConcurrencyConflict(ex);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Found {Count} expired reservations to process"
    )]
    private partial void LogFoundExpiredReservations(int count);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Expired reservation for OrderId {OrderId}, ProductId {ProductId}, Quantity {Quantity}"
    )]
    private partial void LogReservationExpired(Guid orderId, Guid productId, int quantity);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully processed {Count} expired reservations"
    )]
    private partial void LogProcessingCompleted(int count);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Concurrency conflict while processing expired reservations, will retry on next run"
    )]
    private partial void LogConcurrencyConflict(Exception ex);
}
