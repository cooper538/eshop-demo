using EShop.Products.Infrastructure.Data;

namespace EShop.Product.UnitTests.Infrastructure;

public class ProductDataGeneratorTests
{
    [Fact]
    public void GenerateBatches_SingleBatch_ReturnsOneBatch()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 100, batchSize: 500)
            .ToList();

        batches.Should().HaveCount(1);
        batches[0].Products.Should().HaveCount(100);
        batches[0].Stocks.Should().HaveCount(100);
    }

    [Fact]
    public void GenerateBatches_MultipleBatches_ReturnsCorrectCount()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 250, batchSize: 100)
            .ToList();

        batches.Should().HaveCount(3);
        batches[0].Products.Should().HaveCount(100);
        batches[1].Products.Should().HaveCount(100);
        batches[2].Products.Should().HaveCount(50);
    }

    [Fact]
    public void GenerateBatches_TotalProductCount_MatchesRequested()
    {
        const int productCount = 350;

        var totalProducts = ProductDataGenerator
            .GenerateBatches(productCount, batchSize: 100)
            .SelectMany(b => b.Products)
            .ToList();

        totalProducts.Should().HaveCount(productCount);
    }

    [Fact]
    public void GenerateBatches_Products_HaveValidProperties()
    {
        var batch = ProductDataGenerator
            .GenerateBatches(productCount: 100, batchSize: 100)
            .Single();

        foreach (var product in batch.Products)
        {
            product.Id.Should().NotBeEmpty();
            product.Name.Should().NotBeNullOrWhiteSpace();
            product.Description.Should().NotBeNullOrWhiteSpace();
            product.Price.Should().BeInRange(5m, 500m);
            product.Category.Should().NotBeNullOrWhiteSpace();
            product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public void GenerateBatches_Products_HaveUniqueIds()
    {
        var allProducts = ProductDataGenerator
            .GenerateBatches(productCount: 200, batchSize: 100)
            .SelectMany(b => b.Products)
            .ToList();

        allProducts.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateBatches_Stocks_MatchProducts()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 100, batchSize: 100)
            .ToList();

        foreach (var batch in batches)
        {
            batch.Stocks.Should().HaveCount(batch.Products.Count);

            var productIds = batch.Products.Select(p => p.Id).ToHashSet();
            batch.Stocks.Should().AllSatisfy(s => productIds.Should().Contain(s.ProductId));
        }
    }

    [Fact]
    public void GenerateBatches_LastProduct_HasZeroStock()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 250, batchSize: 100)
            .ToList();
        var lastBatch = batches.Last();
        var lastStock = lastBatch.Stocks.Last();

        lastStock.Quantity.Should().Be(0);
    }

    [Fact]
    public void GenerateBatches_NonLastProducts_HaveDefaultStock()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 100, batchSize: 100)
            .ToList();
        var stocks = batches.SelectMany(b => b.Stocks).ToList();

        stocks.SkipLast(1).Should().AllSatisfy(s => s.Quantity.Should().Be(100));
    }

    [Fact]
    public void GenerateBatches_NonLastBatch_AllStocksHaveDefaultQuantity()
    {
        var batches = ProductDataGenerator
            .GenerateBatches(productCount: 200, batchSize: 100)
            .ToList();
        var firstBatch = batches.First();

        firstBatch.Stocks.Should().AllSatisfy(s => s.Quantity.Should().Be(100));
    }
}
