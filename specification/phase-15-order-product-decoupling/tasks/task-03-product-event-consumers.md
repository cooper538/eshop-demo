# Task 03: Product Event Consumers

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ done |
| Dependencies | task-02 |

## Summary
Create MassTransit consumers in Order service for `ProductCreatedEvent` and `ProductUpdatedEvent` that upsert `ProductSnapshot` rows with timestamp-based staleness guard.

## Scope
- [ ] Create `ProductCreatedConsumer` with upsert + timestamp guard logic
- [ ] Create `ProductUpdatedConsumer` with same upsert pattern
- [ ] Register both consumers in `DependencyInjection.cs`
- [ ] Use plain `IConsumer<T>` (not `IdempotentConsumer`) -- upsert is naturally idempotent
- [ ] Explicit `SaveChangesAsync` call (no outbound messages to trigger outbox auto-save)

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: Consumers, Idempotency)
- → PLAN.md (Phase 3 -- consumer pattern, SaveChanges explanation, queue naming)

---
## Notes
(Updated during implementation)
