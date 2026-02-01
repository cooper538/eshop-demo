# Task 08: Order Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ✅ completed |
| Dependencies | task-04, task-05 |

## Summary
Integration tests for Order Service - testing real database, messaging, and the complete domain event pipeline.

## Project Setup
- [x] Create `tests/Order.IntegrationTests/` project
- [x] Create OrderApiFactory (WebApplicationFactory)
- [x] Update solution

## Scope

### EF Core Persistence
- [x] Test entity round-trip (save and load)
- [x] Test owned entities persistence (OrderItems)
- [x] Test concurrency conflict detection
- [x] Test migrations apply correctly (implicit - tests use migrated DB)

### MassTransit Integration
- [x] Test event data mapping
- [N/A] Test outbox pattern - TestHarness uses in-memory transport, bypasses outbox ([GH #3711](https://github.com/MassTransit/MassTransit/discussions/3711))
- [N/A] Test event consumption - Order service has no consumers (only publishers)

### Domain Event Pipeline
- [x] Test full pipeline (command -> domain event -> integration event)
- [x] Test cascading events (OrderLifecycle_PublishesEventsInSequence)
- [N/A] Test transactional rollback - requires custom test setup beyond TestHarness scope

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md)

---
## Notes
- Use Testcontainers for PostgreSQL (from task-04 infrastructure)
- Use MassTransit ITestHarness for messaging tests
- Reset database between tests with Respawn
- Outbox pattern testing not feasible with TestHarness - would require E2E tests with real RabbitMQ
