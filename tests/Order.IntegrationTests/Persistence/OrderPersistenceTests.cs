using EShop.Order.Domain.Entities;
using EShop.Order.Domain.Enums;
using EShop.Order.IntegrationTests.Fixtures;
using EShop.Order.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.IntegrationTests.Persistence;

public class OrderPersistenceTests : OrderIntegrationTestBase
{
    public OrderPersistenceTests(PostgresContainerFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task SaveAndLoad_Order_RoundTripsCorrectly()
    {
        // Arrange
        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "test@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Test Product", 2, 50.00m)],
            DateTime.UtcNow
        );
        order.ClearDomainEvents();

        // Act - Save
        await using (var context = CreateDbContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act - Load (new context to bypass cache)
        await using (var context = CreateDbContext())
        {
            var loaded = await context
                .Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Assert
            loaded.Should().NotBeNull();
            loaded!.Id.Should().Be(order.Id);
            loaded.CustomerId.Should().Be(order.CustomerId);
            loaded.CustomerEmail.Should().Be("test@example.com");
            loaded.Status.Should().Be(EOrderStatus.Created);
            loaded.TotalAmount.Should().Be(100.00m);
            loaded.Items.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task SaveAndLoad_OrderWithMultipleItems_PersistsAllItems()
    {
        // Arrange
        var items = new[]
        {
            OrderItem.Create(Guid.NewGuid(), "Product A", 1, 10.00m),
            OrderItem.Create(Guid.NewGuid(), "Product B", 2, 20.00m),
            OrderItem.Create(Guid.NewGuid(), "Product C", 3, 30.00m),
        };

        var order = OrderEntity.Create(Guid.NewGuid(), "multi@example.com", items, DateTime.UtcNow);
        order.ClearDomainEvents();

        // Act - Save
        await using (var context = CreateDbContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act - Load
        await using (var context = CreateDbContext())
        {
            var loaded = await context
                .Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Assert
            loaded.Should().NotBeNull();
            loaded!.Items.Should().HaveCount(3);
            loaded.TotalAmount.Should().Be(10 + 40 + 90); // 1*10 + 2*20 + 3*30 = 140
            loaded.Items.Should().Contain(i => i.ProductName == "Product A");
            loaded.Items.Should().Contain(i => i.ProductName == "Product B");
            loaded.Items.Should().Contain(i => i.ProductName == "Product C");
        }
    }

    [Fact]
    public async Task Update_OrderStatus_PersistsCorrectly()
    {
        // Arrange
        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "status@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Product", 1, 100.00m)],
            DateTime.UtcNow
        );
        order.ClearDomainEvents();

        await using (var context = CreateDbContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act - Update status
        await using (var context = CreateDbContext())
        {
            var loaded = await context.Orders.FirstAsync(o => o.Id == order.Id);
            loaded.Confirm(DateTime.UtcNow);
            loaded.ClearDomainEvents();
            await context.SaveChangesAsync();
        }

        // Assert - Verify in new context
        await using (var context = CreateDbContext())
        {
            var verified = await context.Orders.FirstAsync(o => o.Id == order.Id);
            verified.Status.Should().Be(EOrderStatus.Confirmed);
            verified.UpdatedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "concurrent@example.com",
            [OrderItem.Create(Guid.NewGuid(), "Product", 1, 100.00m)],
            DateTime.UtcNow
        );
        order.ClearDomainEvents();

        await using (var context = CreateDbContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act - Simulate concurrent update
        await using var context1 = CreateDbContext();
        await using var context2 = CreateDbContext();

        var order1 = await context1.Orders.FirstAsync(o => o.Id == order.Id);
        var order2 = await context2.Orders.FirstAsync(o => o.Id == order.Id);

        // First update succeeds
        order1.Confirm(DateTime.UtcNow);
        order1.ClearDomainEvents();
        await context1.SaveChangesAsync();

        // Second update should fail (order2 has stale version)
        order2.Reject("Concurrent update", DateTime.UtcNow);
        order2.ClearDomainEvents();

        // Assert
        var act = () => context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
