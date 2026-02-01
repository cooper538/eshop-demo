# Task 08: Order Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ⚪ pending |
| Dependencies | task-04, task-05 |

## Summary
Integration tests for Order Service - testing real database, messaging, and the complete domain event pipeline.

## Project Setup
- [ ] Create `tests/Order.IntegrationTests/` project
- [ ] Create OrderApiFactory (WebApplicationFactory)
- [ ] Update solution

## Scope

### EF Core Persistence
- [ ] Test entity round-trip (save and load)
- [ ] Test owned entities persistence (OrderItems)
- [ ] Test concurrency conflict detection
- [ ] Test migrations apply correctly

### MassTransit Integration
- [ ] Test outbox pattern (events persisted in transaction)
- [ ] Test event consumption
- [ ] Test event data mapping

### Domain Event Pipeline
- [ ] Test full pipeline (command -> domain event -> integration event)
- [ ] Test cascading events
- [ ] Test transactional rollback on handler failure

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md)

---
## Notes
- Use Testcontainers for PostgreSQL (from task-04 infrastructure)
- Use MassTransit ITestHarness for messaging tests
- Reset database between tests with Respawn
