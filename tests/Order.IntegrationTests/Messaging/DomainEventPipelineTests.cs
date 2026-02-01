using EShop.Common.IntegrationTests.Fixtures;
using EShop.Contracts.IntegrationEvents.Order;
using EShop.Order.Domain.Entities;
using EShop.Order.IntegrationTests.Infrastructure;

namespace EShop.Order.IntegrationTests.Messaging;

public class DomainEventPipelineTests : OrderIntegrationTestBase
{
    public DomainEventPipelineTests(PostgresContainerFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task ConfirmOrder_PublishesOrderConfirmedEvent()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "pipeline@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Test Product", 2, 50.00m)],
            DateTime.UtcNow
        );

        await using var context = CreateDbContext();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act - Confirm order (triggers domain event -> integration event)
        order.Confirm(DateTime.UtcNow);
        await context.SaveChangesAsync();

        // Assert - Verify integration event was published
        var published = await harness.Published.Any<OrderConfirmedEvent>(x =>
            x.Context.Message.OrderId == order.Id
        );

        published.Should().BeTrue();

        var message = await harness
            .Published.SelectAsync<OrderConfirmedEvent>()
            .FirstOrDefaultAsync(x => x.Context.Message.OrderId == order.Id);

        message.Should().NotBeNull();
        message!.Context.Message.CustomerId.Should().Be(order.CustomerId);
        message.Context.Message.CustomerEmail.Should().Be("pipeline@example.com");
        message.Context.Message.TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task RejectOrder_PublishesOrderRejectedEvent()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "rejected@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Product", 1, 100.00m)],
            DateTime.UtcNow
        );

        await using var context = CreateDbContext();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act - Reject order
        const string reason = "Insufficient stock";
        order.Reject(reason, DateTime.UtcNow);
        await context.SaveChangesAsync();

        // Assert
        var published = await harness.Published.Any<OrderRejectedEvent>(x =>
            x.Context.Message.OrderId == order.Id
        );

        published.Should().BeTrue();

        var message = await harness
            .Published.SelectAsync<OrderRejectedEvent>()
            .FirstOrDefaultAsync(x => x.Context.Message.OrderId == order.Id);

        message.Should().NotBeNull();
        message!.Context.Message.Reason.Should().Be(reason);
    }

    [Fact]
    public async Task CancelOrder_PublishesOrderCancelledEvent()
    {
        // Arrange
        var harness = GetTestHarness();
        await harness.Start();

        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "cancelled@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Product", 1, 100.00m)],
            DateTime.UtcNow
        );
        order.Confirm(DateTime.UtcNow);
        order.ClearDomainEvents();

        await using var context = CreateDbContext();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act - Cancel confirmed order
        const string reason = "Customer request";
        order.Cancel(reason, DateTime.UtcNow);
        await context.SaveChangesAsync();

        // Assert
        var published = await harness.Published.Any<OrderCancelledEvent>(x =>
            x.Context.Message.OrderId == order.Id
        );

        published.Should().BeTrue();

        var message = await harness
            .Published.SelectAsync<OrderCancelledEvent>()
            .FirstOrDefaultAsync(x => x.Context.Message.OrderId == order.Id);

        message.Should().NotBeNull();
        message!.Context.Message.Reason.Should().Be(reason);
    }
}
