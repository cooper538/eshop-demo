# Task 04: Domain Event Dispatcher

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Domain event dispatching via MediatR pipeline behavior with `DomainEventNotification<T>` wrapper.

## Scope
- [x] Create `IDomainEventDispatcher` interface
- [x] Create `MediatRDomainEventDispatcher` implementation
- [x] Create `DomainEventNotification<T>` wrapper for MediatR
- [x] Create `DomainEventDispatchHelper` for collecting events from tracked entities
- [x] Integrate dispatching into `UnitOfWorkExecutor` (dispatches before SaveChanges)
- [x] Domain events auto-registered via `AddApplicationServices()` in each service

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5.4: Publishing Flow)

---
## Notes
- Location: `src/Common/EShop.Common.Application/`
- `MediatRDomainEventDispatcher` wraps domain events in `DomainEventNotification<T>` and publishes via MediatR
- `DomainEventDispatchHelper` collects events from tracked `AggregateRoot` entities and clears them
- `UnitOfWorkExecutor` handles dispatch loop (max 10 iterations to prevent infinite loops)
- Events are cleared BEFORE dispatch to prevent duplicates on retry
- Handlers must be IDEMPOTENT for retry scenarios
