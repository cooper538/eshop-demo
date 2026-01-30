# Task 04: Integration Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Setup shared integration testing infrastructure with Testcontainers, Respawn, and WebApplicationFactory.

## Scope
- [x] Create `tests/Common.IntegrationTests/` project
- [x] Add NuGet packages:
  - [x] Testcontainers, Testcontainers.PostgreSql, Testcontainers.RabbitMq
  - [x] Respawn
  - [x] Microsoft.AspNetCore.Mvc.Testing
- [x] Create `PostgresContainerFixture` class
  - [x] Spin up PostgreSQL container
  - [x] Provide connection string
  - [x] Implement IAsyncLifetime
- [x] Create `RabbitMqContainerFixture` class
  - [x] Spin up RabbitMQ container
  - [x] Provide connection string
- [x] Create `DatabaseFixture` with Respawn
  - [x] Reset database between tests
  - [x] Ignore __EFMigrationsHistory table
- [x] Create `IntegrationTestBase` class
  - [x] Common setup/teardown
  - [x] Database reset helper
- [x] Update `EShopDemo.sln`

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: Integration Testing)

---
## Notes
- Use postgres:16-alpine image for speed
- Use rabbitmq:3-management-alpine image
- Consider CollectionFixture for sharing containers across tests
- Containers should be started once per test run, not per test
