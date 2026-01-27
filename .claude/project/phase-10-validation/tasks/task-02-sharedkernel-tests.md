# Task 02: SharedKernel Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Unit tests for EShop.SharedKernel DDD building blocks (Entity, ValueObject, Guard).

## Scope
- [ ] Test `Entity<TId>` base class
  - [ ] Equality based on Id
  - [ ] GetHashCode consistency
  - [ ] Domain events collection
- [ ] Test `AggregateRoot<TId>`
  - [ ] Domain event registration
  - [ ] ClearDomainEvents behavior
- [ ] Test `ValueObject` base class
  - [ ] Equality based on components
  - [ ] GetHashCode consistency
  - [ ] Null handling
- [ ] Test `Guard` utility class
  - [ ] Guard.Against.Null
  - [ ] Guard.Against.NullOrEmpty
  - [ ] Guard.Against.NegativeOrZero
  - [ ] Guard.Against.OutOfRange

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md) (Section: Test Organization)

---
## Notes
- These are foundational tests - be thorough
- Use Theory/InlineData for parameterized tests where applicable
- Test exception messages for Guard failures
