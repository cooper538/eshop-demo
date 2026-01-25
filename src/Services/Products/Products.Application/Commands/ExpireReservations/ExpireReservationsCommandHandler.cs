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

        var expiredReservations = await _dbContext
            .StockReservations.Where(r =>
                r.Status == EReservationStatus.Active && r.ExpiresAt < now
            )
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (expiredReservations.Count == 0)
        {
            return;
        }

        LogFoundExpiredReservations(expiredReservations.Count);

        // Get all affected products in one query
        var productIds = expiredReservations.Select(r => r.ProductId).Distinct().ToList();
        var products = await _dbContext
            .Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var reservation in expiredReservations)
        {
            if (products.TryGetValue(reservation.ProductId, out var product))
            {
                product.ReleaseStock(reservation.Quantity);
            }
            else
            {
                // Product was deleted - still expire the reservation to prevent reprocessing
                LogProductNotFound(reservation.ProductId, reservation.Id);
            }

            reservation.Expire();
            LogReservationExpired(reservation.OrderId, reservation.ProductId, reservation.Quantity);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            LogProcessingCompleted(expiredReservations.Count);
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
        Level = LogLevel.Warning,
        Message = "Product {ProductId} not found for expired reservation {ReservationId}"
    )]
    private partial void LogProductNotFound(Guid productId, Guid reservationId);

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
