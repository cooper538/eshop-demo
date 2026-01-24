# Task 03: AppHost Project

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |
| Dependencies | task-01 |
| Estimated Effort | M |

## Objective
Create EShop.AppHost project as the Aspire orchestrator with infrastructure resources.

## Scope
- [ ] Create `src/AppHost/EShop.AppHost.csproj` using `Aspire.AppHost.Sdk`
- [ ] Implement `Program.cs` with infrastructure resources:
  - [ ] PostgreSQL with persistent lifetime and pgAdmin
  - [ ] Two databases: `productdb`, `orderdb`
  - [ ] RabbitMQ with persistent lifetime and management plugin
- [ ] Add placeholder comments for future service registrations
- [ ] Add project to solution

## Acceptance Criteria
- [ ] `dotnet run --project src/AppHost` starts successfully
- [ ] Aspire dashboard is accessible
- [ ] PostgreSQL container starts with pgAdmin
- [ ] RabbitMQ container starts with management UI
- [ ] Containers use persistent lifetime (survive restarts)

## Notes
- See spec: aspire-orchestration.md, Section 3 (AppHost Configuration)
- Services will be added in later phases when they exist
- Resource names: `postgres`, `productdb`, `orderdb`, `messaging`
