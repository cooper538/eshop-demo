# Task 05: Initial Data Sync, Tests, and Documentation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ done |
| Dependencies | task-01, task-03, task-04 |

## Summary
Create a startup sync job for cold-start `ProductSnapshot` population, update all affected tests, and update architecture documentation.

## Scope
- [ ] Create `ProductSnapshotSyncJob` (`BackgroundService`) for one-time bootstrap from Product service
- [ ] Register sync job in `DependencyInjection.cs`
- [ ] Update `CreateOrderCommandHandler` unit tests (replace gRPC mock with `ProductSnapshot` seeding)
- [ ] Create consumer unit tests (`ProductCreatedConsumer`, `ProductUpdatedConsumer`) -- new product, idempotency, stale event
- [ ] Update integration tests to seed `ProductSnapshot` rows before order creation
- [ ] Update `docs/architecture.md` -- event flow diagram, materialized view section

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md) (Section: Consumer Testing, Command Handler Testing)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md)
- → PLAN.md (Phases 5, 6, 7 -- sync job details, test modifications, documentation updates)

---
## Notes
(Updated during implementation)
