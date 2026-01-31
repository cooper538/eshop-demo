# Task 08: Order Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ⚪ pending |
| Dependencies | task-04, task-05 |
| Est. Tests | ~12 |

## Summary
Integration tests for Order Service - testing real database, messaging, and the complete domain event pipeline.

## Project Setup
- [ ] Create `tests/Order.IntegrationTests/` project (or use Common.IntegrationTests)
- [ ] Reference Order.Infrastructure, Order.Application projects
- [ ] Create `OrderApiFactory` (WebApplicationFactory)
- [ ] Update `EShopDemo.sln`

## Scope

### EF Core Persistence (~4 tests)
- [ ] `OrderEntity_SaveAndLoad_PreservesAllProperties` - round-trip test
- [ ] `OrderEntity_WithItems_PersistsOwnedEntities` - OrderItems saved with parent
- [ ] `OrderEntity_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException` - version conflict
- [ ] `OrderDbContext_AppliesMigrations_CreatesSchema` - migration test

### MassTransit Integration (~4 tests)
- [ ] `OrderConfirmedEvent_PublishedToOutbox_PersistedInTransaction` - outbox pattern
- [ ] `OrderConfirmedEvent_Consumed_ProcessedByHandler` - consumer works
- [ ] `OrderRejectedEvent_Published_ContainsCorrectData` - event data mapping
- [ ] `OrderCancelledEvent_Published_ContainsCorrectData` - event data mapping

### Domain Event Pipeline (~4 tests)
- [ ] `CreateOrder_TriggersFullPipeline_PublishesIntegrationEvent` - command → domain event → integration event
- [ ] `CancelOrder_TriggersFullPipeline_PublishesIntegrationEvent` - full flow
- [ ] `DomainEvent_WithCascading_HandlesMultipleEvents` - cascading events work
- [ ] `DomainEvent_HandlerFailure_RollsBackTransaction` - transactional consistency

## Test Implementation Notes

```csharp
// Example: Integration test with real database
[Collection("Integration Tests")]
public class OrderPersistenceTests : IntegrationTestBase
{
    [Fact]
    public async Task OrderEntity_SaveAndLoad_PreservesAllProperties()
    {
        // Arrange
        await using var context = CreateDbContext();
        var order = OrderEntity.Create(
            Guid.NewGuid(),
            "test@example.com",
            new[] { OrderItem.Create(Guid.NewGuid(), "Product", 2, 10.00m) }
        );

        // Act
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert - reload from DB
        await using var verifyContext = CreateDbContext();
        var loaded = await verifyContext.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id);

        loaded.CustomerEmail.Should().Be("test@example.com");
        loaded.Items.Should().HaveCount(1);
        loaded.TotalAmount.Should().Be(20.00m);
    }
}

// Example: MassTransit test harness
[Fact]
public async Task OrderConfirmedEvent_Published_ConsumedByHandler()
{
    // Arrange
    var harness = await StartTestHarness();
    var order = await CreateConfirmedOrder();

    // Act - event should be published via outbox
    await TriggerOutboxDelivery();

    // Assert
    (await harness.Published.Any<OrderConfirmedEvent>()).Should().BeTrue();
    var published = harness.Published.Select<OrderConfirmedEvent>().First();
    published.Context.Message.OrderId.Should().Be(order.Id);
}
```

## Related Specs
- [functional-testing.md](../../high-level-specs/functional-testing.md)
- [messaging-communication.md](../../high-level-specs/messaging-communication.md)

---
## Notes
- Use Testcontainers for PostgreSQL (from task-04 infrastructure)
- Use MassTransit ITestHarness for messaging tests
- Reset database between tests with Respawn
- Test outbox pattern - events persisted in same transaction as state changes
- Concurrency test uses Version property from AggregateRoot
