using EShop.Contracts.IntegrationEvents.Product;
using EShop.Products.Domain.Entities;
using EShop.Products.IntegrationTests.Fixtures;
using EShop.Products.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EShop.Products.IntegrationTests.Messaging;

public class DomainEventPipelineTests : ProductIntegrationTestBase
{
    public DomainEventPipelineTests(PostgresContainerFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task CreateProduct_PublishesProductChangedEvent()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var beforeCreate = DateTime.UtcNow;

        var product = ProductEntity.Create(
            "Test Widget",
            "A test product",
            19.99m,
            initialStockQuantity: 50,
            lowStockThreshold: 5,
            "Electronics",
            DateTime.UtcNow
        );

        // Act
        await using var context = CreateDbContext();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert - ProductChangedEvent published via domain event pipeline
        var published = await harness.Published.Any<ProductChangedEvent>(x =>
            x.Context.Message.ProductId == product.Id
        );

        published.Should().BeTrue("ProductCreatedDomainEvent should trigger ProductChangedEvent");

        var message = await harness
            .Published.SelectAsync<ProductChangedEvent>()
            .FirstOrDefaultAsync(x => x.Context.Message.ProductId == product.Id);

        message.Should().NotBeNull();
        message!.Context.Message.Name.Should().Be("Test Widget");
        message.Context.Message.Price.Should().Be(19.99m);
        message.Context.Message.Timestamp.Should().BeOnOrAfter(beforeCreate);
        message.Context.Message.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateProduct_PublishesProductChangedEvent()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var product = ProductEntity.Create(
            "Original Name",
            "A product",
            10.00m,
            initialStockQuantity: 100,
            lowStockThreshold: 10,
            "Office",
            DateTime.UtcNow
        );

        await using var context = CreateDbContext();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act - Update the product
        product.Update(
            "Updated Name",
            "Updated description",
            25.00m,
            "Office",
            lowStockThreshold: 10,
            DateTime.UtcNow
        );
        await context.SaveChangesAsync();

        // Assert - ProductChangedEvent published for update (filter by updated name)
        var published = await harness.Published.Any<ProductChangedEvent>(x =>
            x.Context.Message.ProductId == product.Id && x.Context.Message.Name == "Updated Name"
        );

        published.Should().BeTrue("ProductUpdatedDomainEvent should trigger ProductChangedEvent");

        var message = await harness
            .Published.SelectAsync<ProductChangedEvent>()
            .FirstOrDefaultAsync(x =>
                x.Context.Message.ProductId == product.Id
                && x.Context.Message.Name == "Updated Name"
            );

        message.Should().NotBeNull();
        message!.Context.Message.Price.Should().Be(25.00m);
        message.Context.Message.Timestamp.Should().BeOnOrAfter(beforeUpdate);
        message.Context.Message.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_AlsoCreatesStockViaEventHandler()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var product = ProductEntity.Create(
            "Stock Test Product",
            "Product for stock creation test",
            50.00m,
            initialStockQuantity: 75,
            lowStockThreshold: 15,
            "Electronics",
            DateTime.UtcNow
        );

        // Act
        await using var context = CreateDbContext();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert - Stock entity created by ProductCreatedEventHandler
        var stock = await context.Stocks.FirstOrDefaultAsync(s => s.ProductId == product.Id);

        stock.Should().NotBeNull("ProductCreatedEventHandler should create Stock");
        stock!.Quantity.Should().Be(75);
    }
}
