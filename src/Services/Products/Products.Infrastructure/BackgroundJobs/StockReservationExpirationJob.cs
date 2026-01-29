using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application.Commands.ExpireReservations;
using Products.Application.Configuration;

namespace Products.Infrastructure.BackgroundJobs;

// Background job that expires stale stock reservations and releases stock back to inventory
public sealed partial class StockReservationExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStockReservationOptions _options;
    private readonly ILogger<StockReservationExpirationJob> _logger;

    public StockReservationExpirationJob(
        IServiceProvider serviceProvider,
        IStockReservationOptions options,
        ILogger<StockReservationExpirationJob> logger
    )
    {
        _serviceProvider = serviceProvider;
        _options = options;
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
            catch (ConflictException ex)
            {
                // Concurrency conflict is expected when multiple instances process reservations
                LogConcurrencyConflict(ex);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
            }

            await Task.Delay(_options.Expiration.CheckInterval, stoppingToken);
        }

        LogJobStopped();
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new ExpireReservationsCommand(_options.Expiration.BatchSize),
            cancellationToken
        );
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

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Concurrency conflict while processing expired reservations, will retry on next run"
    )]
    private partial void LogConcurrencyConflict(Exception ex);
}
