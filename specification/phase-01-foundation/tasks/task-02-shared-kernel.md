# Task 2: EShop.SharedKernel

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement clean DDD building blocks without external dependencies.

## Scope
- [x] Create project `EShop.SharedKernel` in `src/Common/EShop.SharedKernel/`
- [x] Implement `Domain/Entity.cs` - base entity with Id and equality
- [x] Implement `Domain/AggregateRoot.cs` - aggregate root with:
  - Domain events collection
  - Version property for optimistic locking
  - IncrementVersion() method
- [x] Implement `Domain/IAggregateRoot.cs` - marker interface
- [x] Implement `Domain/IOwnedEntity.cs` - marker for owned/child entities
- [x] Implement `Domain/ValueObject.cs` - value object with equality components
- [x] Implement `Events/IDomainEvent.cs` - domain event marker interface
- [x] Implement `Events/DomainEventBase.cs` - base record with OccurredOn timestamp
- [x] Implement `Guards/Guard.cs` - guard clauses:
  - Against.Null
  - Against.NullOrEmpty
  - Against.NegativeOrZero
  - Against.Negative
- [x] Implement `Services/IDateTimeProvider.cs` - abstraction for DateTime.UtcNow

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.1 - EShop.SharedKernel)

---
## Notes
- Added IOwnedEntity marker for child entities within aggregates (e.g., OrderItem)
- Added IDateTimeProvider abstraction for testability (avoid DateTime.UtcNow directly)
- Entity does not store domain events - only AggregateRoot does (cleaner design)
