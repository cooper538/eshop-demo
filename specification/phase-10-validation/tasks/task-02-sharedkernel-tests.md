# Task 02: SharedKernel Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |
| Est. Tests | ~18 |

## Summary
Unit tests for EShop.SharedKernel DDD building blocks - foundational classes used across all services.

## Scope

### Entity Base Class (~4 tests)
- [ ] `Entity_WithSameId_AreEqual` - two entities with same Id are equal
- [ ] `Entity_WithDifferentId_AreNotEqual` - different Ids means not equal
- [ ] `Entity_ComparedToNull_IsNotEqual` - null comparison returns false
- [ ] `Entity_GetHashCode_ConsistentWithId` - hash based on Id

### AggregateRoot (~5 tests)
- [ ] `AggregateRoot_AddDomainEvent_AddsToCollection` - event added to DomainEvents
- [ ] `AggregateRoot_AddMultipleEvents_AllPresent` - multiple events tracked
- [ ] `AggregateRoot_ClearDomainEvents_RemovesAll` - events cleared after dispatch
- [ ] `AggregateRoot_Version_StartsAtZero` - initial version is 0
- [ ] `AggregateRoot_IncrementVersion_IncreasesByOne` - version increments

### ValueObject (~5 tests)
- [ ] `ValueObject_WithSameComponents_AreEqual` - same values = equal
- [ ] `ValueObject_WithDifferentComponents_AreNotEqual` - different values = not equal
- [ ] `ValueObject_EqualityOperator_Works` - `==` operator
- [ ] `ValueObject_InequalityOperator_Works` - `!=` operator
- [ ] `ValueObject_GetHashCode_ConsistentWithComponents` - hash from components

### Guard Utility (~4 tests)
- [ ] `Guard_AgainstNull_ThrowsArgumentNullException` - null throws
- [ ] `Guard_AgainstNullOrEmpty_ThrowsArgumentException` - empty string throws
- [ ] `Guard_AgainstNegativeOrZero_ThrowsArgumentOutOfRangeException` - zero/negative throws
- [ ] `Guard_AgainstNegative_ThrowsArgumentOutOfRangeException` - negative throws

## Test Implementation Notes

```csharp
// Example: Entity equality test
[Fact]
public void Entity_WithSameId_AreEqual()
{
    // Arrange
    var id = Guid.NewGuid();
    var entity1 = new TestEntity(id);
    var entity2 = new TestEntity(id);

    // Act & Assert
    entity1.Should().Be(entity2);
}

// Example: Guard test with exception message
[Fact]
public void Guard_AgainstNull_ThrowsArgumentNullException()
{
    // Arrange
    string? value = null;

    // Act
    var act = () => Guard.Against.Null(value, nameof(value));

    // Assert
    act.Should().Throw<ArgumentNullException>()
       .WithParameterName("value");
}
```

## Related Specs
- [unit-testing.md](../../high-level-specs/unit-testing.md)

---
## Notes
- Create test entity/value object classes for testing (TestEntity, TestValueObject)
- Use `[Theory, InlineData]` for parameterized Guard tests
- Test exception parameter names for better debugging
- These tests validate DDD infrastructure used by all services
