# Task 04: Environment Detection

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | pending |
| Dependencies | - |

## Summary
~~Add environment detection extensions to distinguish between local Aspire development and Azure Container Apps deployment.~~

**SIMPLIFIED**: Use built-in .NET environment detection. No custom code needed.

## Environment Strategy

| Environment | `ASPNETCORE_ENVIRONMENT` | Detection | Use case |
|-------------|--------------------------|-----------|----------|
| Local (Aspire) | `Development` | `IsDevelopment()` | Local dev with Docker containers |
| Azure | `Production` | `IsProduction()` | Azure Container Apps deployment |

## Scope
- [x] ~~Create `EnvironmentExtensions.cs`~~ - NOT NEEDED, use built-in `IsDevelopment()` / `IsProduction()`
- [ ] Update documentation to reflect environment strategy
- [ ] Ensure `ASPNETCORE_ENVIRONMENT=Production` is set in Container Apps deployment

## Usage Pattern

```csharp
// Program.cs - simple and clean
if (builder.Environment.IsProduction())
{
    // Azure: Key Vault, SSL connections, FQDN service URLs
    builder.AddKeyVaultConfiguration();
    builder.AddNpgsqlDbContextAzure<ProductDbContext>("productdb");
}
else
{
    // Local: Aspire service discovery, no SSL required
    builder.AddNpgsqlDbContext<ProductDbContext>("productdb");
}

// Messaging works the same in both environments (RabbitMQ)
builder.AddMessaging<ProductDbContext>("product");
```

## Configuration Files

| Environment | Config file loaded |
|-------------|-------------------|
| Development | `service.settings.yaml` + `service.settings.Development.yaml` |
| Production | `service.settings.yaml` + `service.settings.Production.yaml` |

## Files to Create/Modify

| Action | File |
|--------|------|
| NONE | No code changes needed - use built-in .NET methods |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)

---
## Notes
- Simplified from original spec - no custom `IsAzure()` method needed
- Built-in `IsDevelopment()` and `IsProduction()` are sufficient
- Set `ASPNETCORE_ENVIRONMENT=Production` in Container Apps environment variables
- Optional: `CONTAINER_APP_NAME` env var is set automatically by Container Apps if specific detection needed
