namespace EShop.DatabaseMigration;

/// <summary>
/// Stops the host after all migrations are complete.
/// Waits for MigrationTracker to signal that all DbInitializers have finished.
/// </summary>
public sealed class MigrationCompletionService : BackgroundService
{
    private readonly MigrationTracker _tracker;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<MigrationCompletionService> _logger;

    public MigrationCompletionService(
        MigrationTracker tracker,
        IHostApplicationLifetime lifetime,
        ILogger<MigrationCompletionService> logger
    )
    {
        _tracker = tracker;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _tracker.AllCompletedTask.WaitAsync(stoppingToken);
            _logger.LogInformation(
                "All database migrations completed. Stopping migration service."
            );
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Migration completion service was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed. Stopping migration service.");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
