# Task 04: Domain Event Dispatcher

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | :white_circle: pending |
| Dependencies | task-01 |

## Summary
Create EF Core SaveChangesInterceptor to dispatch domain events via MediatR before committing.

## Scope
- [ ] Create `DomainEventDispatcherInterceptor` extending `SaveChangesInterceptor`
- [ ] Override `SavingChangesAsync` to dispatch events before save
- [ ] Get all `AggregateRoot` entities from ChangeTracker with pending domain events
- [ ] Clear domain events before dispatching to prevent duplicates
- [ ] Dispatch each event via `IMediator.Publish()`
- [ ] Register interceptor in `OrderDbContext` DI configuration
- [ ] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5.4: Publishing Flow)

---
## Notes
(Updated during implementation)
