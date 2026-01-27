# Task 02: SharedKernel Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Comprehensive unit tests for EShop.SharedKernel core abstractions.

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

## Dependencies
- Depends on: task-01
- Blocks: none

## Acceptance Criteria
- [ ] All Guard methods have at least one positive and one negative test
- [ ] Entity equality tests cover edge cases
- [ ] ValueObject tests verify structural equality

## Notes
- These are foundational tests - be thorough
- Use Theory/InlineData for parameterized tests where applicable
- Test exception messages for Guard failures
