using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Products.Infrastructure.Data;

public class ProductDatabaseSeeder : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ProductDatabaseSeeder> _logger;

    public ProductDatabaseSeeder(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<ProductDatabaseSeeder> logger
    )
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogDebug("Skipping database seeding in non-development environment");
            return;
        }

        _logger.LogInformation("Starting product database seeding...");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            await ProductDbContextSeed.SeedAsync(context, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the product database");
        }
    }
}
