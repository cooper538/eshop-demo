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
- [ ] Create `IDomainEvent` marker interface (extends MediatR's `INotification`)
- [ ] Create `AggregateRoot` base class extending `Entity`
- [ ] Add private `_domainEvents` list with public readonly accessor
- [ ] Add `AddDomainEvent(IDomainEvent)` protected method
- [ ] Add `ClearDomainEvents()` public method
- [ ] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5: Outbox Pattern)

---
## Notes
(Updated during implementation)
