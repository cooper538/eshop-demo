# Task 03: AppHost Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Objective
Create EShop.AppHost project as the Aspire orchestrator with full infrastructure and service configuration.

## Scope
- [x] Create `src/AppHost/EShop.AppHost.csproj` using `Aspire.AppHost.Sdk/13.1.0`
- [x] Implement `Program.cs` with infrastructure resources:
  - [x] PostgreSQL with persistent lifetime and pgAdmin
  - [x] Three databases: `productdb`, `orderdb`, `notificationdb`
  - [x] RabbitMQ with persistent lifetime and management plugin
- [x] Configure all services with dependencies:
  - [x] Migration service (runs first, applies DB migrations)
  - [x] Product service (waits for migration, uses productdb + rabbitmq)
  - [x] Order service (waits for migration, uses orderdb + rabbitmq, references product)
  - [x] Notification service (waits for migration, uses notificationdb + rabbitmq)
  - [x] Analytics service (uses rabbitmq only)
  - [x] Gateway (references product + order, external HTTP endpoints)
- [x] Implement `DockerComposeExtensions.cs` for publishing support

## Acceptance Criteria
- [x] `dotnet run --project src/AppHost` starts successfully
- [x] Aspire dashboard accessible with all services visible
- [x] PostgreSQL starts with pgAdmin UI
- [x] RabbitMQ starts with management UI
- [x] Services start in correct order (migration first)
- [x] Docker Compose publishing generates valid compose file

## Implementation Details

### Service Dependencies
```
Migration Service
    ↓ (WaitForCompletion)
┌───┴───┬───────────┬────────────┐
│       │           │            │
Product Order   Notification  Analytics
   ↓       ↓
Gateway (external)
```

### Docker Compose Restart Policies
| Service | Restart Policy |
|---------|----------------|
| Postgres, RabbitMQ | unless-stopped |
| Migration | no (one-time job) |
| All other services | on-failure |

### Project References
```xml
<ProjectReference Include="../Services/Products/Products.API/Products.API.csproj" />
<ProjectReference Include="../Services/Order/Order.API/Order.API.csproj" />
<ProjectReference Include="../Services/Notification/EShop.NotificationService.csproj" />
<ProjectReference Include="../Services/Analytics/EShop.AnalyticsService.csproj" />
<ProjectReference Include="../Services/Gateway/Gateway.API/Gateway.API.csproj" />
<ProjectReference Include="../Services/DatabaseMigration/EShop.DatabaseMigration.csproj" />
```

## Notes
- See spec: aspire-orchestration.md, Section 3 (AppHost Configuration)
- Resource names defined in `EShop.ServiceDefaults.ResourceNames`
- ASPIRE004 warning suppressed (required for this SDK version)
