# Task 06: Order.UnitTests Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Create unit test project for Order Service.

## Scope
- [ ] Create `tests/Order.UnitTests/` project
- [ ] Reference Order.Domain, Order.Application projects
- [ ] Add NuGet packages: xUnit, FluentAssertions, Moq
- [ ] Create folder structure:
  - [ ] `Domain/` for entity tests
  - [ ] `Application/Commands/` for command handler tests
  - [ ] `Application/Queries/` for query handler tests
  - [ ] `Application/Validators/` for validator tests
  - [ ] `Helpers/` for test utilities
- [ ] Add `TestDbContextFactory` helper for InMemory EF Core
- [ ] Update `EShopDemo.sln`

## Dependencies
- Depends on: task-01
- Blocks: task-07

## Acceptance Criteria
- [ ] `dotnet build tests/Order.UnitTests` succeeds
- [ ] Project references correct Order Service layers
- [ ] TestDbContextFactory creates valid InMemory context

## Notes
- Order Service has complex state machine - prepare for extensive tests
- TestDbContextFactory should use unique database names per test
