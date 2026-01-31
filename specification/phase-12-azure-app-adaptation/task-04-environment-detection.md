# Task 04: Environment Detection

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | pending |
| Dependencies | - |

## Summary
Add environment detection extensions to distinguish between local Aspire development and Azure Container Apps deployment.

## Scope
- [ ] Create `EnvironmentExtensions.cs` in `EShop.Common.Infrastructure/Hosting/`
- [ ] Implement `IsAzure()` extension method for `IHostEnvironment`
- [ ] Implement `IsLocal()` extension method for `IHostEnvironment`
- [ ] Add `IsContainerApp()` detection for Container Apps specific features
- [ ] Create `AzureEnvironmentNames` constants class
- [ ] Update `ServiceDefaults` to expose environment detection

## Implementation Notes

```csharp
// EnvironmentExtensions.cs
public static class EnvironmentExtensions
{
    public const string AzureEnvironmentName = "Azure";

    /// <summary>
    /// Returns true when running in Azure (Container Apps, App Service, etc.)
    /// </summary>
    public static bool IsAzure(this IHostEnvironment environment)
    {
        // Check for Azure environment name
        if (environment.EnvironmentName.Equals(
            AzureEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check for Container Apps environment variable
        if (!string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable("CONTAINER_APP_NAME")))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true when running locally (Development environment without Azure indicators)
    /// </summary>
    public static bool IsLocal(this IHostEnvironment environment)
    {
        return environment.IsDevelopment() && !environment.IsAzure();
    }

    /// <summary>
    /// Returns true when running in Azure Container Apps
    /// </summary>
    public static bool IsContainerApp(this IHostEnvironment environment)
    {
        return !string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"));
    }
}

// AzureEnvironmentNames.cs
public static class AzureEnvironmentNames
{
    public const string Azure = "Azure";
    public const string Development = "Development";
    public const string Production = "Production";
}
```

## Container Apps Environment Variables

| Variable | Description |
|----------|-------------|
| CONTAINER_APP_NAME | Name of the Container App |
| CONTAINER_APP_REVISION | Current revision name |
| CONTAINER_APP_ENV_DNS_SUFFIX | Environment DNS suffix |

## Usage Example

```csharp
// Program.cs
if (builder.Environment.IsAzure())
{
    builder.AddKeyVaultConfiguration();
    builder.AddDatabaseAzure<OrderDbContext>("orderdb");
}
else
{
    builder.AddNpgsqlDbContext<OrderDbContext>("orderdb");
}

// Messaging works the same in both environments (RabbitMQ on ACI in Azure)
builder.AddMessaging<OrderDbContext>("order");
```

## Files to Create/Modify

| Action | File |
|--------|------|
| CREATE | `src/Common/EShop.Common.Infrastructure/Hosting/EnvironmentExtensions.cs` |
| CREATE | `src/Common/EShop.Common.Infrastructure/Hosting/AzureEnvironmentNames.cs` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)

---
## Notes
- ASPNETCORE_ENVIRONMENT should be set to "Azure" in Container Apps configuration
- Container Apps sets CONTAINER_APP_NAME automatically
