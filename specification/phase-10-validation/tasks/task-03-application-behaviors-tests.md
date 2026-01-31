# Task 03: Application Behaviors Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |
| Est. Tests | ~17 |

## Summary
Unit tests for EShop.Common.Application behaviors - the core pipeline infrastructure (UnitOfWork, domain events, correlation).

## Scope

### UnitOfWorkExecutor (~5 tests)
- [ ] `Execute_WithDomainEvents_DispatchesAllEvents` - events dispatched before save
- [ ] `Execute_WithCascadingEvents_DispatchesInLoop` - nested events handled
- [ ] `Execute_ExceedsMaxIterations_ThrowsException` - max 10 loops protection
- [ ] `Execute_WithConcurrencyException_ThrowsConflictException` - DbUpdateConcurrencyException mapped
- [ ] `Execute_NoEvents_SavesWithoutDispatch` - no events = direct save

### DomainEventDispatchHelper (~4 tests)
- [ ] `DispatchDomainEvents_CollectsFromAggregates_DispatchesAll` - finds events in change tracker
- [ ] `DispatchDomainEvents_ClearsEventsBeforeDispatch_PreventsInfiniteLoop` - events cleared first
- [ ] `DispatchDomainEvents_NoAggregates_ReturnsZero` - empty tracker = 0 events
- [ ] `DispatchDomainEvents_MultipleAggregates_CollectsFromAll` - events from multiple roots

### MediatRDomainEventDispatcher (~3 tests)
- [ ] `Dispatch_WrapsEventInNotification_PublishesViaMediatR` - DomainEventNotification<T> created
- [ ] `Dispatch_CachesConstructorInfo_PerformanceOptimization` - reflection cached
- [ ] `Dispatch_MultipleEvents_PublishesAll` - all events published

### CorrelationContext (~3 tests)
- [ ] `CreateScope_SetsCurrentContext_AccessibleInScope` - scope sets Current
- [ ] `CreateScope_Disposed_RestoresPreviousContext` - previous context restored
- [ ] `CreateScope_Nested_MaintainsIsolation` - nested scopes work correctly

### CorrelationIdAccessor (~2 tests)
- [ ] `GetCorrelationId_WithContext_ReturnsContextId` - reads from CorrelationContext
- [ ] `GetCorrelationId_NoContext_ReturnsNewGuid` - fallback to new GUID

## Test Implementation Notes

```csharp
// Example: UnitOfWorkExecutor cascading events test
[Fact]
public async Task Execute_WithCascadingEvents_DispatchesInLoop()
{
    // Arrange - event handler that raises another event
    var aggregate = new TestAggregate();
    aggregate.RaiseEvent(new FirstEvent()); // Handler raises SecondEvent

    // Act
    await _executor.ExecuteAsync(...);

    // Assert - both events dispatched
    _mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<FirstEvent>(), ...), Times.Once);
    _mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<SecondEvent>(), ...), Times.Once);
}

// Example: CorrelationContext scope test
[Fact]
public void CreateScope_Disposed_RestoresPreviousContext()
{
    // Arrange
    var originalId = "original-123";
    using var _ = CorrelationContext.CreateScope(originalId);

    // Act
    using (CorrelationContext.CreateScope("nested-456"))
    {
        CorrelationContext.Current?.CorrelationId.Should().Be("nested-456");
    }

    // Assert - original restored
    CorrelationContext.Current?.CorrelationId.Should().Be("original-123");
}
```

## Related Specs
- [unit-testing.md](../../high-level-specs/unit-testing.md)
- [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- Mock IChangeTrackerAccessor, IDomainEventDispatcher, IUnitOfWork
- Test async flow for CorrelationContext (AsyncLocal behavior)
- UnitOfWorkExecutor is critical - handles transaction boundaries
- Max iteration limit (10) prevents infinite event loops
