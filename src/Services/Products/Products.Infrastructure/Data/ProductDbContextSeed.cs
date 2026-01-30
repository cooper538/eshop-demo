using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data;

public static class ProductDbContextSeed
{
    public static void Seed(DbContext context)
    {
        if (context is not ProductDbContext db)
        {
            return;
        }

        var logger = GetLogger(context);

        if (db.Products.Any())
        {
            logger.LogDebug("Database already seeded, skipping");
            return;
        }

        logger.LogInformation("Seeding product database...");

        var now = DateTime.UtcNow;
        var products = CreateSeedProducts(now);
        var stocks = CreateSeedStocks(products);

        db.Products.AddRange(products);
        db.Stocks.AddRange(stocks);
        db.SaveChanges();

        logger.LogInformation(
            "Seeded {ProductCount} products and {StockCount} stocks",
            products.Count,
            stocks.Count
        );
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (context is not ProductDbContext db)
        {
            return;
        }

        var logger = GetLogger(context);

        if (await db.Products.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Database already seeded, skipping");
            return;
        }

        logger.LogInformation("Seeding product database...");

        var now = DateTime.UtcNow;
        var products = CreateSeedProducts(now);
        var stocks = CreateSeedStocks(products);

        db.Products.AddRange(products);
        db.Stocks.AddRange(stocks);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded {ProductCount} products and {StockCount} stocks",
            products.Count,
            stocks.Count
        );
    }

    private static ILogger GetLogger(DbContext context)
    {
        var loggerFactory = context.GetService<ILoggerFactory>();
        return loggerFactory.CreateLogger(typeof(ProductDbContextSeed));
    }

    private static List<ProductEntity> CreateSeedProducts(DateTime createdAt) =>
        [
            CreateProduct(
                "Wireless Mouse",
                "Ergonomic wireless mouse with USB receiver",
                29.99m,
                "Electronics",
                createdAt
            ),
            CreateProduct(
                "Mechanical Keyboard",
                "RGB mechanical keyboard with Cherry MX switches",
                149.99m,
                "Electronics",
                createdAt
            ),
            CreateProduct(
                "USB-C Hub",
                "7-in-1 USB-C hub with HDMI and SD card reader",
                49.99m,
                "Electronics",
                createdAt
            ),
            CreateProduct(
                "Laptop Stand",
                "Adjustable aluminum laptop stand",
                39.99m,
                "Accessories",
                createdAt
            ),
            CreateProduct(
                "Webcam HD",
                "1080p webcam with built-in microphone",
                79.99m,
                "Electronics",
                createdAt
            ),
            CreateProduct(
                "Desk Lamp",
                "LED desk lamp with adjustable brightness",
                34.99m,
                "Office",
                createdAt
            ),
            CreateProduct(
                "Monitor Arm",
                "Single monitor arm with VESA mount",
                89.99m,
                "Accessories",
                createdAt
            ),
            CreateProduct(
                "Headphone Stand",
                "Wooden headphone stand",
                24.99m,
                "Accessories",
                createdAt
            ),
            CreateProduct(
                "Cable Management Kit",
                "Complete cable organizer set",
                19.99m,
                "Office",
                createdAt
            ),
            CreateProduct(
                "Wireless Charger",
                "15W fast wireless charging pad",
                29.99m,
                "Electronics",
                createdAt
            ),
        ];

    private static List<StockEntity> CreateSeedStocks(List<ProductEntity> products) =>
        products
            .Select(
                (product, index) =>
                    CreateStock(
                        product.Id,
                        initialQuantity: index == 9 ? 0 : 100, // Last product (Wireless Charger) has 0 stock
                        lowStockThreshold: 10
                    )
            )
            .ToList();

    private static ProductEntity CreateProduct(
        string name,
        string description,
        decimal price,
        string category,
        DateTime createdAt
    )
    {
        var product = CreateInstance<ProductEntity>();

        SetProperty(product, "Id", Guid.NewGuid());
        SetProperty(product, "Name", name);
        SetProperty(product, "Description", description);
        SetProperty(product, "Price", price);
        SetProperty(product, "Category", category);
        SetProperty(product, "CreatedAt", createdAt);

        return product;
    }

    private static StockEntity CreateStock(
        Guid productId,
        int initialQuantity,
        int lowStockThreshold
    )
    {
        var stock = CreateInstance<StockEntity>();

        SetProperty(stock, "Id", Guid.NewGuid());
        SetProperty(stock, "ProductId", productId);
        SetProperty(stock, "Quantity", initialQuantity);
        SetProperty(stock, "LowStockThreshold", lowStockThreshold);

        return stock;
    }

    private static T CreateInstance<T>()
        where T : class => (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;

    private static void SetProperty<T>(T instance, string propertyName, object value)
        where T : class
    {
        var property = typeof(T).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (property == null)
        {
            // Try base types
            var type = typeof(T).BaseType;
            while (type != null && property == null)
            {
                property = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );
                type = type.BaseType;
            }
        }

        property?.SetValue(instance, value);
    }
}
