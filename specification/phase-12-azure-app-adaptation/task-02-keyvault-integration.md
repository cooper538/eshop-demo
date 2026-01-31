# Task 02: Key Vault Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | pending |
| Dependencies | - |

## Summary
Integrate Azure Key Vault configuration provider with DefaultAzureCredential for secure secret management in Azure environment.

## Scope
- [ ] Add `Azure.Extensions.AspNetCore.Configuration.Secrets` and `Azure.Identity` packages to `Directory.Packages.props`
- [ ] Create `KeyVaultExtensions.cs` in `EShop.Common.Infrastructure/Configuration/`
- [ ] Implement `AddKeyVaultConfiguration()` extension method
- [ ] Use `DefaultAzureCredential` for authentication (supports Azure CLI locally, Managed Identity in Azure)
- [ ] Configure Key Vault URI from configuration/environment variable
- [ ] Add graceful fallback when Key Vault is not configured (local development)
- [ ] Map Key Vault secret names to configuration keys

## Implementation Notes

```csharp
// KeyVaultExtensions.cs
public static class KeyVaultExtensions
{
    public static IHostApplicationBuilder AddKeyVaultConfiguration(
        this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["KeyVault:Uri"];

        if (string.IsNullOrEmpty(keyVaultUri))
        {
            // Key Vault not configured - skip (local development)
            return builder;
        }

        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential(),
            new KeyVaultSecretManager());

        return builder;
    }
}

// Custom secret manager for mapping secret names to config keys
public class EShopSecretManager : KeyVaultSecretManager
{
    public override string GetKey(KeyVaultSecret secret)
    {
        // Map Key Vault names to configuration paths
        // e.g., "ConnectionStrings--productdb" -> "ConnectionStrings:productdb"
        return secret.Name.Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
```

## Key Vault Secrets Mapping

| Key Vault Secret Name | Configuration Key |
|----------------------|-------------------|
| ConnectionStrings--productdb | ConnectionStrings:productdb |
| ConnectionStrings--orderdb | ConnectionStrings:orderdb |
| ConnectionStrings--notificationdb | ConnectionStrings:notificationdb |
| ConnectionStrings--catalogdb | ConnectionStrings:catalogdb |
| ConnectionStrings--messaging | ConnectionStrings:messaging |
| SendGrid--ApiKey | SendGrid:ApiKey |

## Files to Create/Modify

| Action | File |
|--------|------|
| MODIFY | `Directory.Packages.props` |
| CREATE | `src/Common/EShop.Common.Infrastructure/Configuration/KeyVaultExtensions.cs` |
| CREATE | `src/Common/EShop.Common.Infrastructure/Configuration/EShopSecretManager.cs` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.2 Key Vault)

---
## Notes
- DefaultAzureCredential authentication chain: Environment -> Managed Identity -> Azure CLI -> Visual Studio
- Key Vault URI format: `https://{vault-name}.vault.azure.net/`
