# Task 08: Notification.UnitTests Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Create unit test project for Notification Service with MassTransit test harness setup.

## Scope
- [ ] Create `tests/Notification.UnitTests/` project
- [ ] Reference Notification.Worker project
- [ ] Add NuGet packages:
  - [ ] xUnit, FluentAssertions, Moq
  - [ ] MassTransit.Testing
- [ ] Create folder structure:
  - [ ] `Consumers/` for consumer tests
  - [ ] `Services/` for email service tests
  - [ ] `Helpers/` for test utilities
- [ ] Setup MassTransit InMemory test harness base class
- [ ] Update `EShopDemo.sln`

## Dependencies
- Depends on: task-01
- Blocks: task-09

## Acceptance Criteria
- [ ] `dotnet build tests/Notification.UnitTests` succeeds
- [ ] MassTransit test harness initializes correctly
- [ ] Sample consumer test runs (can be placeholder)

## Notes
- MassTransit.Testing provides ITestHarness for in-memory message testing
- Setup harness in test fixture (IAsyncLifetime)
- Consider AddMassTransitTestHarness extension
