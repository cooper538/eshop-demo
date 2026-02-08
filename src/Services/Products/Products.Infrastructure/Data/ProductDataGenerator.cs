using System.Reflection;
using Bogus;
using EShop.Products.Domain.Entities;

namespace EShop.Products.Infrastructure.Data;

public record SeedBatch(List<ProductEntity> Products, List<StockEntity> Stocks);

// Bypasses DDD private constructors/setters via reflection to seed entities without triggering domain events
public static class ProductDataGenerator
{
    private const int DefaultStockQuantity = 100;
    private const int DefaultLowStockThreshold = 10;
    private const decimal MinPrice = 5m;
    private const decimal MaxPrice = 500m;

    private static readonly string[] Categories =
    [
        "Electronics",
        "Accessories",
        "Office",
        "Audio",
        "Networking",
        "Storage",
        "Gaming",
        "Peripherals",
    ];

    public static IEnumerable<SeedBatch> GenerateBatches(int productCount, int batchSize)
    {
        var faker = CreateFaker(DateTime.UtcNow);
        var generated = 0;

        while (generated < productCount)
        {
            var batchCount = Math.Min(batchSize, productCount - generated);
            var isLastBatch = generated + batchCount >= productCount;

            var products = faker.Generate(batchCount);
            var stocks = GenerateStocks(products, isLastBatch);

            yield return new SeedBatch(products, stocks);
            generated += batchCount;
        }
    }

    private static Faker<ProductEntity> CreateFaker(DateTime createdAt)
    {
        return new Faker<ProductEntity>()
            .CustomInstantiator(_ => CreateInstance<ProductEntity>())
            .RuleFor(nameof(ProductEntity.Id), f => Guid.NewGuid())
            .RuleFor(nameof(ProductEntity.Name), f => f.Commerce.ProductName())
            .RuleFor(
                nameof(ProductEntity.Description),
                f =>
                    $"{f.Commerce.ProductAdjective()} {f.Commerce.ProductMaterial()} - {f.Lorem.Sentence()}"
            )
            .RuleFor(
                nameof(ProductEntity.Price),
                f => Math.Round(f.Random.Decimal(MinPrice, MaxPrice), 2)
            )
            .RuleFor(nameof(ProductEntity.Category), f => f.PickRandom(Categories))
            .RuleFor(nameof(ProductEntity.CreatedAt), _ => createdAt);
    }

    private static List<StockEntity> GenerateStocks(List<ProductEntity> products, bool isLastBatch)
    {
        var stocks = new List<StockEntity>(products.Count);

        for (var i = 0; i < products.Count; i++)
        {
            var isOutOfStock = isLastBatch && i == products.Count - 1;
            var quantity = isOutOfStock ? 0 : DefaultStockQuantity;
            stocks.Add(CreateStock(products[i].Id, quantity, DefaultLowStockThreshold));
        }

        return stocks;
    }

    private static StockEntity CreateStock(
        Guid productId,
        int initialQuantity,
        int lowStockThreshold
    )
    {
        var stock = CreateInstance<StockEntity>();

        SetProperty(stock, nameof(StockEntity.Id), Guid.NewGuid());
        SetProperty(stock, nameof(StockEntity.ProductId), productId);
        SetProperty(stock, nameof(StockEntity.Quantity), initialQuantity);
        SetProperty(stock, nameof(StockEntity.LowStockThreshold), lowStockThreshold);

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
