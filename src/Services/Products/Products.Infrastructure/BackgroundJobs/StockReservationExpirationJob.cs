using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application.Commands.ExpireReservations;

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
        LogJobStarted();

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
                LogProcessingError(ex);
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        LogJobStopped();
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new ExpireReservationsCommand(BatchSize), cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock reservation expiration job started"
    )]
    private partial void LogJobStarted();

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock reservation expiration job stopped"
    )]
    private partial void LogJobStopped();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing expired reservations")]
    private partial void LogProcessingError(Exception ex);
}
