# Task 03: AppHost Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Objective
Create EShop.AppHost project as the Aspire orchestrator with infrastructure resources.

## Scope
- [x] Create `src/AppHost/EShop.AppHost.csproj` using `Aspire.AppHost.Sdk`
- [x] Implement `Program.cs` with infrastructure resources:
  - [x] PostgreSQL with persistent lifetime and pgAdmin
  - [x] Two databases: `productdb`, `orderdb`
  - [x] RabbitMQ with persistent lifetime and management plugin
- [x] Add placeholder comments for future service registrations
- [x] Add project to solution

## Acceptance Criteria
- [x] `dotnet run --project src/AppHost` starts successfully
- [x] Aspire dashboard is accessible
- [x] PostgreSQL container starts with pgAdmin
- [x] RabbitMQ container starts with management UI
- [x] Containers use persistent lifetime (survive restarts)

## Notes
- See spec: aspire-orchestration.md, Section 3 (AppHost Configuration)
- Services will be added in later phases when they exist
- Resource names: `postgres`, `productdb`, `orderdb`, `messaging`
- ASPIRE002 warning will resolve when service projects are added in task-04
