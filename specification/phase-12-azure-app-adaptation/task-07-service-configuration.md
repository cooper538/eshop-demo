# Task 07: Service Configuration Updates

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | pending |
| Dependencies | task-01, task-02, task-03, task-04, task-05, task-06 |

## Summary
Update all services to use environment-aware configuration that switches between local (Aspire/RabbitMQ) and Azure (Service Bus/Key Vault) based on environment detection.

## Scope
- [ ] Update `Product.API/Program.cs` with environment-aware configuration
- [ ] Update `Order.API/Program.cs` with environment-aware configuration
- [ ] Update `NotificationService/Program.cs` with environment-aware configuration
- [ ] Update `Gateway.API/Program.cs` with environment-aware configuration
- [ ] Update `Catalog.API/Program.cs` with environment-aware configuration
- [ ] Add `appsettings.Azure.yaml` to each service with Azure-specific configuration
- [ ] Create shared `AddAzureInfrastructure()` extension for common Azure setup
- [ ] Update `DependencyInjection.cs` in each service to use environment-aware methods

## Implementation Notes

```csharp
// Common pattern for each service's Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.AddYamlConfiguration("product");
builder.AddServiceDefaults();
builder.AddSerilog();

// Environment-aware infrastructure
if (builder.Environment.IsAzure())
{
    builder.AddKeyVaultConfiguration();
    builder.AddDatabaseAzure<ProductDbContext>("productdb");
    builder.AddMessagingAzure<ProductDbContext>("product");
}
else
{
    // Local development with Aspire
    builder.AddNpgsqlDbContext<ProductDbContext>("productdb");
    builder.AddMessaging<ProductDbContext>("product");
}

builder.Services.AddProductServices();
```

## Service Configuration Matrix

| Service | Database | Messaging | gRPC Client | Notes |
|---------|----------|-----------|-------------|-------|
| Product.API | productdb | Publisher | - | Stock events |
| Order.API | orderdb | Publisher | product-service | Order events |
| Notification | notificationdb | Consumer | - | All event consumers |
| Gateway.API | - | - | - | YARP reverse proxy |
| Catalog.API | catalogdb | - | product-service | Read model sync |

## Azure Configuration Files

Each service needs `appsettings.Azure.yaml`:

```yaml
# Product.API/appsettings.Azure.yaml
Azure:
  Enabled: true

KeyVault:
  Uri: https://eshop-keyvault.vault.azure.net/

Logging:
  LogLevel:
    Default: Information
    Microsoft.AspNetCore: Warning
```

## Files to Create/Modify

| Action | File |
|--------|------|
| MODIFY | `src/Services/Product/Product.API/Program.cs` |
| MODIFY | `src/Services/Order/Order.API/Program.cs` |
| MODIFY | `src/Services/Notification/NotificationService/Program.cs` |
| MODIFY | `src/Services/Gateway/Gateway.API/Program.cs` |
| MODIFY | `src/Services/Catalog/Catalog.API/Program.cs` |
| CREATE | `src/Services/Product/Product.API/appsettings.Azure.yaml` |
| CREATE | `src/Services/Order/Order.API/appsettings.Azure.yaml` |
| CREATE | `src/Services/Notification/NotificationService/appsettings.Azure.yaml` |
| CREATE | `src/Services/Gateway/Gateway.API/appsettings.Azure.yaml` |
| CREATE | `src/Services/Catalog/Catalog.API/appsettings.Azure.yaml` |
| CREATE | `src/Common/EShop.Common.Infrastructure/Hosting/AzureInfrastructureExtensions.cs` |

## Verification

1. **Local Development**: `dotnet run --project src/AppHost` works unchanged
2. **Azure Config**: Set `ASPNETCORE_ENVIRONMENT=Azure` and verify Key Vault, Service Bus, and PostgreSQL SSL are used
3. **Build**: `dotnet build EShopDemo.sln` passes

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)
- -> [aspire-orchestration.md](../high-level-specs/aspire-orchestration.md)

---
## Notes
- This task integrates all previous Phase 12 tasks
- AppHost (Aspire) configuration remains unchanged - only individual services get Azure awareness
