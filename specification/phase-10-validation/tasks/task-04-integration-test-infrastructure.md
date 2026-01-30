# Task 04: Integration Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Setup shared integration testing infrastructure with Testcontainers, Respawn, and WebApplicationFactory.

## Scope
- [ ] Create `tests/Common.IntegrationTests/` project
- [ ] Add NuGet packages:
  - [ ] Testcontainers, Testcontainers.PostgreSql, Testcontainers.RabbitMq
  - [ ] Respawn
  - [ ] Microsoft.AspNetCore.Mvc.Testing
- [ ] Create `PostgresContainerFixture` class
  - [ ] Spin up PostgreSQL container
  - [ ] Provide connection string
  - [ ] Implement IAsyncLifetime
- [ ] Create `RabbitMqContainerFixture` class
  - [ ] Spin up RabbitMQ container
  - [ ] Provide connection string
- [ ] Create `DatabaseFixture` with Respawn
  - [ ] Reset database between tests
  - [ ] Ignore __EFMigrationsHistory table
- [ ] Create `IntegrationTestBase` class
  - [ ] Common setup/teardown
  - [ ] Database reset helper
- [ ] Update `EShopDemo.sln`

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: Integration Testing)

---
## Notes
- Use postgres:16-alpine image for speed
- Use rabbitmq:3-management-alpine image
- Consider CollectionFixture for sharing containers across tests
- Containers should be started once per test run, not per test
