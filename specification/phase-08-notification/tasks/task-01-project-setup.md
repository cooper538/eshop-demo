# Task 1: Project Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create Worker Service project with Aspire integration for processing notification events.

## Scope
- [x] Create `EShop.NotificationService` Worker Service project in `src/Services/Notification/`
- [x] Add project reference to solution (`EShopDemo.sln`)
- [x] Reference `EShop.ServiceDefaults` for Aspire integration
- [x] Reference `EShop.Contracts` for integration events
- [x] Reference `EShop.Common.Infrastructure` for correlation ID handling
- [x] Add MassTransit packages (`MassTransit`, `MassTransit.RabbitMQ`)
- [x] Add EF Core packages (`Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`)
- [x] Implement `Program.cs` with `AddServiceDefaults()`, `AddSerilog()`, and health checks
- [x] Implement `DependencyInjection.cs` with MassTransit and database configuration
- [x] Configure RabbitMQ connection via Aspire resource injection
- [x] Configure retry policy (1s, 5s, 15s intervals)
- [x] Add correlation ID filters for distributed tracing

## Implementation
```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);
builder.AddYamlConfiguration("notification");
builder.AddServiceDefaults();
builder.AddSerilog();
builder.Services.AddHealthChecks().AddPostgresHealthCheck("notificationdb");
builder.AddNotificationServices();
```

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 5. Service Integration)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 8.2. Consumer Configuration)

---
## Notes
- Uses `Microsoft.NET.Sdk.Worker` SDK
- YAML configuration support via `NetEscapades.Configuration.Yaml`
- Endpoint naming: `KebabCaseEndpointNameFormatter("notification", false)`
