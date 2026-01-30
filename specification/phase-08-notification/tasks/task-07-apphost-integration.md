# Task 7: AppHost Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-05, task-06 |

## Summary
Register Notification Service in Aspire orchestration and verify end-to-end event flow.

## Scope
- [x] Add project reference to `EShop.AppHost.csproj`
- [x] Register `notification-service` in AppHost `Program.cs`
- [x] Add reference to `rabbitmq` (messaging)
- [x] Add reference to `notificationdb` (PostgreSQL database)
- [x] Configure `WaitForCompletion(migrationService)` for database readiness
- [x] Configure `WaitFor(rabbitmq)` for messaging readiness
- [x] Service included in Docker Compose publishing configuration

## Implementation

### AppHost Registration
```csharp
var notificationDb = postgres.AddDatabase(ResourceNames.Databases.Notification);

var notificationService = builder
    .AddProject<Projects.EShop_NotificationService>("notification-service")
    .WithReference(notificationDb)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);
```

### Resource Dependencies
| Dependency | Type | Purpose |
|------------|------|---------|
| notificationdb | PostgreSQL | ProcessedMessages inbox storage |
| rabbitmq | RabbitMQ | Message bus for events |
| migrationService | Project | Database schema migration |

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 3.2. Orchestration Code)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (Section: 3.3. Resource Dependencies)

---
## Notes
- No HTTP/HTTPS endpoints (worker service only)
- Database migrations handled by centralized `DatabaseMigration` service
- Service visible in Aspire dashboard as `notification-service`
