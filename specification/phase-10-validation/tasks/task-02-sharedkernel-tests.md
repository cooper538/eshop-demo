# Task 02: SharedKernel Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |

## Summary
Unit tests for EShop.SharedKernel DDD building blocks (Entity, ValueObject, Guard).

## Scope
- [ ] Test `Entity` base class (uses `Guid Id`)
  - [ ] Equality based on Id
  - [ ] GetHashCode consistency
- [ ] Test `AggregateRoot` (extends Entity)
  - [ ] Domain event registration via `AddDomainEvent()`
  - [ ] ClearDomainEvents behavior
  - [ ] Version property for optimistic locking
- [ ] Test `ValueObject` base class
  - [ ] Equality based on components (`GetEqualityComponents()`)
  - [ ] GetHashCode consistency
  - [ ] Null handling
- [ ] Test `Guard` utility class
  - [ ] Guard.Against.Null<T>
  - [ ] Guard.Against.NullOrEmpty (string)
  - [ ] Guard.Against.NegativeOrZero (decimal)
  - [ ] Guard.Against.Negative (decimal)

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md) (Section: Test Organization)

---
## Notes
- These are foundational tests - be thorough
- Use Theory/InlineData for parameterized tests where applicable
- Test exception messages for Guard failures
