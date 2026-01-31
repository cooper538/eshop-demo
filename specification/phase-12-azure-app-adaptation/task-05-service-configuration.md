# Task 05: Service Configuration Updates

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | pending |
| Dependencies | task-01, task-02, task-03 |

## Summary
Update all services to use environment-aware configuration that switches between local (Aspire) and Azure (Key Vault/PostgreSQL SSL) based on `IsProduction()`. Messaging (RabbitMQ) requires no changes - same config works in both environments.

## Scope
- [ ] Update `Product.API/Program.cs` with environment-aware configuration
- [ ] Update `Order.API/Program.cs` with environment-aware configuration
- [ ] Update `Notification/Program.cs` with environment-aware configuration
- [ ] Update `Gateway.API/Program.cs` with environment-aware configuration
- [ ] Update `Catalog.API/Program.cs` with environment-aware configuration (if exists)
- [ ] Add `*.settings.Production.yaml` to each service with Azure-specific configuration
- [ ] Create shared `AddAzureInfrastructure()` extension for common Azure setup (optional)

## Implementation Notes

```csharp
// Common pattern for each service's Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.AddYamlConfiguration("product");
builder.AddServiceDefaults();
builder.AddSerilog();

// Environment-aware infrastructure
if (builder.Environment.IsProduction())
{
    // Azure: Key Vault for secrets, SSL for PostgreSQL
    builder.AddKeyVaultConfiguration();
    builder.AddNpgsqlDbContextAzure<ProductDbContext>("productdb");
}
else
{
    // Local development with Aspire
    builder.AddNpgsqlDbContext<ProductDbContext>("productdb");
}

// Messaging works the same in both environments (RabbitMQ)
builder.AddMessaging<ProductDbContext>("product");

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

## Production Configuration Files

Each service needs `*.settings.Production.yaml`:

```yaml
# product.settings.Production.yaml
KeyVault:
  Uri: https://eshop-keyvault.vault.azure.net/
  ManagedIdentityClientId: <user-assigned-mi-client-id>  # optional

Logging:
  LogLevel:
    Default: Information
    Microsoft.AspNetCore: Warning
```

```yaml
# order.settings.Production.yaml
KeyVault:
  Uri: https://eshop-keyvault.vault.azure.net/

ServiceClients:
  ProductService:
    Url: https://product-service.internal.eshop-env.westeurope.azurecontainerapps.io
```

## Files to Create/Modify

| Action | File |
|--------|------|
| MODIFY | `src/Services/Products/Products.API/Program.cs` |
| MODIFY | `src/Services/Order/Order.API/Program.cs` |
| MODIFY | `src/Services/Notification/Program.cs` |
| MODIFY | `src/Services/Gateway/Gateway.API/Program.cs` |
| CREATE | `src/Services/Products/Products.API/product.settings.Production.yaml` |
| CREATE | `src/Services/Order/Order.API/order.settings.Production.yaml` |
| CREATE | `src/Services/Notification/notification.settings.Production.yaml` |
| CREATE | `src/Services/Gateway/Gateway.API/gateway.settings.Production.yaml` |

## Verification

1. **Local Development**: `dotnet run --project src/AppHost` works unchanged
2. **Production Config**: Set `ASPNETCORE_ENVIRONMENT=Production` and verify Key Vault and PostgreSQL SSL are used
3. **Build**: `dotnet build EShopDemo.sln` passes

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)
- -> [aspire-orchestration.md](../high-level-specs/aspire-orchestration.md)

---
## Notes
- This task integrates all previous Phase 12 tasks
- AppHost (Aspire) configuration remains unchanged - only individual services get Azure awareness
- Environment detection uses built-in `IsProduction()` - no custom code needed
- Config files: `*.settings.Production.yaml` (not `appsettings.Azure.yaml`)
