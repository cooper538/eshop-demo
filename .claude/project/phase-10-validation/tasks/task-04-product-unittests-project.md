# Task 04: Product.UnitTests Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Create unit test project for Product Service.

## Scope
- [ ] Create `tests/Product.UnitTests/` project
- [ ] Reference Product.Domain, Product.Application projects
- [ ] Add NuGet packages: xUnit, FluentAssertions, Moq
- [ ] Create folder structure:
  - [ ] `Domain/` for entity tests
  - [ ] `Application/Commands/` for command handler tests
  - [ ] `Application/Queries/` for query handler tests
  - [ ] `Application/Validators/` for validator tests
  - [ ] `Grpc/` for gRPC service tests
  - [ ] `Helpers/` for test utilities
- [ ] Add `TestServerCallContext` helper for gRPC tests
- [ ] Update `EShopDemo.sln`

## Dependencies
- Depends on: task-01
- Blocks: task-05

## Acceptance Criteria
- [ ] `dotnet build tests/Product.UnitTests` succeeds
- [ ] Project references correct Product Service layers
- [ ] Folder structure matches spec

## Notes
- Keep test project structure mirroring source project
- TestServerCallContext is needed for gRPC service unit tests
