using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EShop.Products.Infrastructure.Data;

public static class ProductDbContextSeed
{
    public static async Task SeedAsync(
        ProductDbContext db,
        int productCount = 1_000,
        int batchSize = 500,
        CancellationToken cancellationToken = default
    )
    {
        var logger = GetLogger(db);

        if (await db.Products.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Database already seeded, skipping");
            return;
        }

        logger.LogInformation(
            "Seeding {ProductCount} products (batch size: {BatchSize})...",
            productCount,
            batchSize
        );

        var seeded = 0;

        foreach (var batch in ProductDataGenerator.GenerateBatches(productCount, batchSize))
        {
            db.Products.AddRange(batch.Products);
            db.Stocks.AddRange(batch.Stocks);
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();

            seeded += batch.Products.Count;
            logger.LogDebug("Seeded {Seeded}/{Total} products", seeded, productCount);
        }

        logger.LogInformation("Seeded {ProductCount} products", productCount);
    }

    private static ILogger GetLogger(DbContext context)
    {
        var loggerFactory = context.GetService<ILoggerFactory>();
        return loggerFactory.CreateLogger(typeof(ProductDbContextSeed));
    }
}
