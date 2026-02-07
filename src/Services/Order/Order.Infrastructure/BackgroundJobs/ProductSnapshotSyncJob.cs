using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Domain.ReadModels;
using EShop.Order.Infrastructure.Data;
using EShop.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.BackgroundJobs;

public sealed class ProductSnapshotSyncJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductSnapshotSyncJob> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var productClient = scope.ServiceProvider.GetRequiredService<IProductServiceClient>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var hasSnapshots = await dbContext.ProductSnapshots.AnyAsync(stoppingToken);

            if (hasSnapshots)
            {
                logger.LogInformation("ProductSnapshots table is not empty, skipping initial sync");
                return;
            }

            logger.LogInformation(
                "ProductSnapshots table is empty, starting initial sync from Product service"
            );

            var result = await productClient.GetAllProductsAsync(stoppingToken);

            if (result.Products.Count == 0)
            {
                logger.LogWarning("No products returned from Product service during initial sync");
                return;
            }

            var now = dateTimeProvider.UtcNow;
            foreach (var product in result.Products)
            {
                dbContext.ProductSnapshots.Add(
                    ProductSnapshot.Create(product.ProductId, product.Name, product.Price, now)
                );
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            logger.LogInformation(
                "Initial ProductSnapshot sync completed: {Count} products synced",
                result.Products.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to sync ProductSnapshots on startup. Events will populate data going forward"
            );
        }
    }
}
