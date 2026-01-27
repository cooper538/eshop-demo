# Task 10: Integration Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-10 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Setup shared integration testing infrastructure with Testcontainers and Respawn.

## Scope
- [ ] Create `tests/Common.IntegrationTests/` project (or shared infrastructure)
- [ ] Add NuGet packages:
  - [ ] Testcontainers
  - [ ] Testcontainers.PostgreSql
  - [ ] Testcontainers.RabbitMq
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

## Dependencies
- Depends on: task-01
- Blocks: task-11, task-12, task-13

## Acceptance Criteria
- [ ] PostgreSQL container starts and provides valid connection
- [ ] RabbitMQ container starts and provides valid connection
- [ ] Respawn resets database correctly
- [ ] Base classes are reusable across service test projects

## Notes
- Use postgres:16-alpine image for speed
- Use rabbitmq:3-management-alpine image
- Consider CollectionFixture for sharing containers across tests
- Containers should be started once per test run, not per test
