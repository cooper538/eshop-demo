using EShop.Contracts.ServiceClients.Product;
using EShop.Order.Domain.ReadModels;
using EShop.Order.Infrastructure.BackgroundJobs;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;
using EShop.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Order.UnitTests.Infrastructure.BackgroundJobs;

public class ProductSnapshotSyncJobTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;
    private readonly Mock<IProductServiceClient> _productClientMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<ILogger<ProductSnapshotSyncJob>> _loggerMock = new();
    private readonly DateTime _fixedTime = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public ProductSnapshotSyncJobTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedTime);
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    private ProductSnapshotSyncJob CreateJob(OrderDbContext context)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(OrderDbContext))).Returns(context);
        serviceProvider
            .Setup(x => x.GetService(typeof(IProductServiceClient)))
            .Returns(_productClientMock.Object);
        serviceProvider
            .Setup(x => x.GetService(typeof(IDateTimeProvider)))
            .Returns(_dateTimeProviderMock.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

        return new ProductSnapshotSyncJob(scopeFactory.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTableEmpty_SyncsProductsFromService()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var job = CreateJob(context);

        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        _productClientMock
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new GetProductsResult([
                    new ProductInfo(productId1, "Product A", "Desc A", 10.00m, 100),
                    new ProductInfo(productId2, "Product B", "Desc B", 25.50m, 50),
                ])
            );

        // Act
        await job.StartAsync(CancellationToken.None);
        await job.ExecuteTask!;

        // Assert
        var snapshots = await context.ProductSnapshots.ToListAsync();
        snapshots.Should().HaveCount(2);
        snapshots
            .Should()
            .Contain(s => s.ProductId == productId1 && s.Name == "Product A" && s.Price == 10.00m);
        snapshots
            .Should()
            .Contain(s => s.ProductId == productId2 && s.Name == "Product B" && s.Price == 25.50m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTableNotEmpty_SkipsSync()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        context.ProductSnapshots.Add(
            ProductSnapshot.Create(Guid.NewGuid(), "Existing", 5.00m, DateTime.UtcNow)
        );
        await context.SaveChangesAsync();

        var job = CreateJob(context);

        // Act
        await job.StartAsync(CancellationToken.None);
        await job.ExecuteTask!;

        // Assert
        _productClientMock.Verify(
            x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsEmpty_DoesNotSaveAnything()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var job = CreateJob(context);

        _productClientMock
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetProductsResult([]));

        // Act
        await job.StartAsync(CancellationToken.None);
        await job.ExecuteTask!;

        // Assert
        var snapshots = await context.ProductSnapshots.ToListAsync();
        snapshots.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrows_DoesNotCrash()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var job = CreateJob(context);

        _productClientMock
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        await job.StartAsync(CancellationToken.None);
        var act = () => job.ExecuteTask!;

        // Assert - job completes gracefully without throwing
        await act.Should().NotThrowAsync();
        var snapshots = await context.ProductSnapshots.ToListAsync();
        snapshots.Should().BeEmpty();
    }
}
