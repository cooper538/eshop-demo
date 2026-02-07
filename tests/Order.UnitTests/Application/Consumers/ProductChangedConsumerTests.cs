using EShop.Contracts.IntegrationEvents.Product;
using EShop.Order.Application.Consumers;
using EShop.Order.Domain.ReadModels;
using EShop.Order.Infrastructure.Data;
using EShop.Order.UnitTests.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.UnitTests.Application.Consumers;

public class ProductChangedConsumerTests : IDisposable
{
    private readonly TestDbContextFactory _dbContextFactory;
    private readonly Mock<ILogger<ProductChangedConsumer>> _loggerMock = new();
    private readonly DateTime _fixedTime = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public ProductChangedConsumerTests()
    {
        _dbContextFactory = new TestDbContextFactory();
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    private ProductChangedConsumer CreateConsumer(OrderDbContext context)
    {
        return new ProductChangedConsumer(context, context, _loggerMock.Object);
    }

    private static Mock<ConsumeContext<ProductChangedEvent>> CreateConsumeContext(
        ProductChangedEvent message
    )
    {
        var mock = new Mock<ConsumeContext<ProductChangedEvent>>();
        mock.Setup(x => x.Message).Returns(message);
        mock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        return mock;
    }

    [Fact]
    public async Task Consume_NewProduct_CreatesSnapshot()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var consumer = CreateConsumer(context);
        var productId = Guid.NewGuid();
        var message = new ProductChangedEvent(productId, "Widget", 19.99m)
        {
            Timestamp = _fixedTime,
        };

        // Act
        await consumer.Consume(CreateConsumeContext(message).Object);

        // Assert
        var snapshot = await context.ProductSnapshots.FindAsync(productId);
        snapshot.Should().NotBeNull();
        snapshot!.Name.Should().Be("Widget");
        snapshot.Price.Should().Be(19.99m);
        snapshot.LastUpdated.Should().Be(_fixedTime);
    }

    [Fact]
    public async Task Consume_ExistingProduct_UpdatesSnapshot()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var consumer = CreateConsumer(context);
        var productId = Guid.NewGuid();

        context.ProductSnapshots.Add(
            ProductSnapshot.Create(productId, "Old Name", 10.00m, _fixedTime)
        );
        await context.SaveChangesAsync();

        var laterTime = _fixedTime.AddMinutes(5);
        var message = new ProductChangedEvent(productId, "New Name", 25.00m)
        {
            Timestamp = laterTime,
        };

        // Act
        await consumer.Consume(CreateConsumeContext(message).Object);

        // Assert
        var snapshot = await context.ProductSnapshots.FindAsync(productId);
        snapshot!.Name.Should().Be("New Name");
        snapshot.Price.Should().Be(25.00m);
        snapshot.LastUpdated.Should().Be(laterTime);
    }

    [Fact]
    public async Task Consume_StaleEvent_SkipsUpdate()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateContext();
        var consumer = CreateConsumer(context);
        var productId = Guid.NewGuid();

        context.ProductSnapshots.Add(
            ProductSnapshot.Create(productId, "Current Name", 30.00m, _fixedTime)
        );
        await context.SaveChangesAsync();

        var olderTime = _fixedTime.AddMinutes(-5);
        var message = new ProductChangedEvent(productId, "Stale Name", 15.00m)
        {
            Timestamp = olderTime,
        };

        // Act
        await consumer.Consume(CreateConsumeContext(message).Object);

        // Assert
        var snapshot = await context.ProductSnapshots.FindAsync(productId);
        snapshot!.Name.Should().Be("Current Name");
        snapshot.Price.Should().Be(30.00m);
    }
}
