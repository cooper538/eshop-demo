using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.Data;
using Products.Domain.Entities;
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

        // First, get IDs of expired reservations (batch limit applied to reservations, not stocks)
        var expiredReservationIds = await _dbContext
            .Stocks.SelectMany(s => s.Reservations)
            .Where(r => r.Status == EReservationStatus.Active && r.ExpiresAt < now)
            .OrderBy(r => r.ExpiresAt)
            .Take(request.BatchSize)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (expiredReservationIds.Count == 0)
        {
            return;
        }

        LogFoundExpiredReservations(expiredReservationIds.Count);

        // Load stocks with only the selected expired reservations
        var stocks = await _dbContext
            .Stocks.Include(s => s.Reservations.Where(r => expiredReservationIds.Contains(r.Id)))
            .Where(s => s.Reservations.Any(r => expiredReservationIds.Contains(r.Id)))
            .ToListAsync(cancellationToken);

        var totalExpired = 0;

        foreach (var stock in stocks)
        {
            // Reservations are already filtered to only expired ones
            foreach (var reservation in stock.Reservations)
            {
                stock.ExpireReservation(reservation);
                LogReservationExpired(
                    reservation.OrderId,
                    reservation.ProductId,
                    reservation.Quantity
                );
                totalExpired++;
            }
        }

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
