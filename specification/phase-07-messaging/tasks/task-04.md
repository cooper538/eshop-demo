# Task 04: Domain Event Dispatcher

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Enable domain event dispatching in Order service via existing MediatR pipeline behavior.

## Scope
- [x] Register `AddDomainEvents()` in Order.API to enable `IDomainEventDispatcher`
- [x] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5.4: Publishing Flow)

---
## Notes
Domain event dispatching was already implemented in EShop.Common as MediatR pipeline behavior (`DomainEventDispatchBehavior`). Only needed to add `AddDomainEvents()` registration to Order.API/Program.cs.
