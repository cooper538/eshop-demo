using EShop.Products.Domain.Entities;
using EShop.Products.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.E2E.Tests.Fixtures;

public static class TestDataSeeder
{
    public static async Task SeedProductsAsync(
        ProductDbContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var testProducts = new[]
        {
            ProductEntity.Create(
                "Available Product A",
                "Test product for E2E tests",
                99.99m,
                initialStockQuantity: 100,
                lowStockThreshold: 10,
                "Electronics",
                now
            ),
            ProductEntity.Create(
                "Available Product B",
                "Another test product",
                49.99m,
                initialStockQuantity: 50,
                lowStockThreshold: 5,
                "Accessories",
                now
            ),
            ProductEntity.Create(
                "Unavailable Product",
                "Product with zero stock for testing insufficient stock scenarios",
                29.99m,
                initialStockQuantity: 0,
                lowStockThreshold: 10,
                "Electronics",
                now
            ),
        };

        context.Products.AddRange(testProducts);

        var stocks = testProducts
            .Select(
                (p, i) =>
                    StockEntity.Create(
                        p.Id,
                        initialQuantity: i == 2 ? 0 : (i + 1) * 50,
                        lowStockThreshold: 10
                    )
            )
            .ToArray();

        context.Stocks.AddRange(stocks);

        await context.SaveChangesAsync(cancellationToken);
    }
}
