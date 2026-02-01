# Task 02: Key Vault Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Integrate Azure Key Vault configuration provider with DefaultAzureCredential for secure secret management in Azure environment.

## Scope
- [x] Add `Azure.Extensions.AspNetCore.Configuration.Secrets` and `Azure.Identity` packages
- [x] Create `AddKeyVaultConfiguration()` extension method
- [x] Use `DefaultAzureCredential` for authentication (Azure CLI locally, Managed Identity in Azure)
- [x] Configure graceful fallback when Key Vault is not configured (local development)
- [x] Support user-assigned MI via `KeyVault:ManagedIdentityClientId` config key

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.2 Key Vault)

---
## Notes
- DefaultAzureCredential chain: Environment -> Managed Identity -> Azure CLI -> Visual Studio
- Key Vault secret names use `--` separator (translated to `:` by config provider)
