# Task 01: AggregateRoot Base Class

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create AggregateRoot base class with domain events collection support in EShop.SharedKernel.

## Scope
- [x] Create `IDomainEvent` marker interface (simple marker, NOT extending MediatR's `INotification`)
- [x] Create `DomainEventBase` abstract record with `OccurredOn` property
- [x] Create `AggregateRoot` base class extending `Entity`
- [x] Add private `_domainEvents` list with public readonly accessor
- [x] Add `AddDomainEvent(IDomainEvent)` protected method
- [x] Add `ClearDomainEvents()` public method
- [x] Add `Version` property for optimistic locking
- [x] Add `IncrementVersion()` protected method
- [x] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5: Outbox Pattern)

---
## Notes
- Location: `src/Common/EShop.SharedKernel/`
- `IDomainEvent` is a simple marker interface (not extending MediatR)
- `DomainEventBase` provides base implementation with `OccurredOn` timestamp
- `AggregateRoot` also includes `Version` for concurrency control
