using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application.Data;
using Products.Domain.Enums;

namespace Products.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that expires stale stock reservations and releases stock back to inventory.
/// Runs every minute to check for Active reservations where ExpiresAt has passed.
/// </summary>
public sealed partial class StockReservationExpirationJob : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockReservationExpirationJob> _logger;

    public StockReservationExpirationJob(
        IServiceProvider serviceProvider,
        ILogger<StockReservationExpirationJob> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogJobStarted(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogProcessingError(_logger, ex);
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        LogJobStopped(_logger);
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IProductDbContext>();

        var now = DateTime.UtcNow;

        var expiredReservations = await db
            .StockReservations.Where(r =>
                r.Status == EReservationStatus.Active && r.ExpiresAt < now
            )
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (expiredReservations.Count == 0)
        {
            return;
        }

        LogFoundExpiredReservations(_logger, expiredReservations.Count);

        // Get all affected products in one query
        var productIds = expiredReservations.Select(r => r.ProductId).Distinct().ToList();
        var products = await db
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
                LogProductNotFound(_logger, reservation.ProductId, reservation.Id);
            }

            reservation.Expire();
            LogReservationExpired(
                _logger,
                reservation.OrderId,
                reservation.ProductId,
                reservation.Quantity
            );
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            LogProcessingCompleted(_logger, expiredReservations.Count);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Another instance processed some reservations - will retry on next run
            LogConcurrencyConflict(_logger, ex);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock reservation expiration job started"
    )]
    private static partial void LogJobStarted(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock reservation expiration job stopped"
    )]
    private static partial void LogJobStopped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing expired reservations")]
    private static partial void LogProcessingError(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Found {Count} expired reservations to process"
    )]
    private static partial void LogFoundExpiredReservations(ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Product {ProductId} not found for expired reservation {ReservationId}"
    )]
    private static partial void LogProductNotFound(
        ILogger logger,
        Guid productId,
        Guid reservationId
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Expired reservation for OrderId {OrderId}, ProductId {ProductId}, Quantity {Quantity}"
    )]
    private static partial void LogReservationExpired(
        ILogger logger,
        Guid orderId,
        Guid productId,
        int quantity
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully processed {Count} expired reservations"
    )]
    private static partial void LogProcessingCompleted(ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Concurrency conflict while processing expired reservations, will retry on next run"
    )]
    private static partial void LogConcurrencyConflict(ILogger logger, Exception ex);
}
