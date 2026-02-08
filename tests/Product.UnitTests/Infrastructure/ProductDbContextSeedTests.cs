using EShop.Product.UnitTests.Helpers;
using EShop.Products.Infrastructure.Data;

namespace EShop.Product.UnitTests.Infrastructure;

public class ProductDbContextSeedTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory = new();

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SeedAsync_EmptyDatabase_SeedsRequestedProductCount()
    {
        const int productCount = 100;

        await using var context = _dbContextFactory.CreateContext();
        await ProductDbContextSeed.SeedAsync(context, productCount, batchSize: 100);

        context.Products.Should().HaveCount(productCount);
        context.Stocks.Should().HaveCount(productCount);
    }

    [Fact]
    public async Task SeedAsync_DatabaseAlreadySeeded_SkipsSeeding()
    {
        const int initialCount = 100;

        await using var context = _dbContextFactory.CreateContext();
        await ProductDbContextSeed.SeedAsync(context, initialCount, batchSize: 100);

        await ProductDbContextSeed.SeedAsync(context, productCount: 200, batchSize: 100);

        context.Products.Should().HaveCount(initialCount);
    }

    [Fact]
    public async Task SeedAsync_WithMultipleBatches_SeedsAllProducts()
    {
        const int productCount = 250;
        const int batchSize = 100;

        await using var context = _dbContextFactory.CreateContext();
        await ProductDbContextSeed.SeedAsync(context, productCount, batchSize);

        context.Products.Should().HaveCount(productCount);
        context.Stocks.Should().HaveCount(productCount);
    }

    [Fact]
    public async Task SeedAsync_ExactlyOneProduct_HasZeroStock()
    {
        await using var context = _dbContextFactory.CreateContext();
        await ProductDbContextSeed.SeedAsync(context, productCount: 100, batchSize: 100);

        var stocks = context.Stocks.ToList();
        stocks.Where(s => s.Quantity == 0).Should().ContainSingle();
        stocks.Where(s => s.Quantity == 100).Should().HaveCount(99);
    }

    [Fact]
    public async Task SeedAsync_EachProductHasMatchingStock()
    {
        await using var context = _dbContextFactory.CreateContext();
        await ProductDbContextSeed.SeedAsync(context, productCount: 100, batchSize: 50);

        var productIds = context.Products.Select(p => p.Id).ToHashSet();
        var stockProductIds = context.Stocks.Select(s => s.ProductId).ToHashSet();

        stockProductIds.Should().BeEquivalentTo(productIds);
    }

    [Fact]
    public async Task SeedAsync_CancellationRequested_StopsSeeding()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await using var context = _dbContextFactory.CreateContext();

        var act = () =>
            ProductDbContextSeed.SeedAsync(context, productCount: 100, batchSize: 50, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
