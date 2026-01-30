using Microsoft.EntityFrameworkCore;

namespace EShop.DatabaseMigration;

public sealed class DbInitializer<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MigrationTracker _tracker;
    private readonly ILogger<DbInitializer<TContext>> _logger;

    public DbInitializer(
        IServiceScopeFactory scopeFactory,
        MigrationTracker tracker,
        ILogger<DbInitializer<TContext>> logger
    )
    {
        _scopeFactory = scopeFactory;
        _tracker = tracker;
        _logger = logger;

        _tracker.Register(typeof(TContext).Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var contextName = typeof(TContext).Name;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        _logger.LogInformation("Applying migrations for {ContextName}...", contextName);

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
                await context.Database.MigrateAsync(stoppingToken)
            );
            _logger.LogInformation(
                "Migrations applied successfully for {ContextName}",
                contextName
            );
            _tracker.MarkCompleted(contextName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply migrations for {ContextName}", contextName);
            _tracker.MarkFailed(contextName, ex);
            throw;
        }
    }
}
