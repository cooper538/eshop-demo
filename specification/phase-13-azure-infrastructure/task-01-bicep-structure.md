# Task 01: Bicep Project Structure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | pending |
| Dependencies | - |

## Summary
Set up the `infra/` folder structure with Bicep modules, main orchestration file, and parameter files for different environments.

## Scope
- [ ] Create `infra/` folder structure in project root
- [ ] Create `main.bicep` entry point with module orchestration
- [ ] Create `modules/` folder for resource modules
- [ ] Create `parameters/` folder with `dev.bicepparam` and `prod.bicepparam`
- [ ] Define common parameters (location, naming prefix, tags)
- [ ] Create `bicepconfig.json` for linting and module aliases
- [ ] Add `.gitignore` entries for Bicep build outputs

## Folder Structure

```
infra/
├── main.bicep                 # Entry point - orchestrates all modules
├── bicepconfig.json           # Linting rules, module aliases
├── modules/
│   ├── identity.bicep         # Managed Identity + Role Assignments
│   ├── monitoring.bicep       # Log Analytics Workspace
│   ├── postgres.bicep         # PostgreSQL Flexible Server + DBs
│   ├── service-bus.bicep      # Service Bus Namespace + Queues
│   ├── key-vault.bicep        # Key Vault + Secrets
│   ├── acr.bicep              # Container Registry
│   └── container-apps.bicep   # Container Apps Environment + Apps
└── parameters/
    ├── dev.bicepparam         # Development environment
    └── prod.bicepparam        # Production environment
```

## Main Bicep Structure

```bicep
// main.bicep
targetScope = 'subscription'

@description('Environment name (dev, prod)')
param environment string

@description('Azure region for resources')
param location string = 'westeurope'

@description('Resource naming prefix')
param prefix string = 'eshop'

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: '${prefix}-${environment}-rg'
  location: location
  tags: {
    Environment: environment
    Project: 'EShop Demo'
    ManagedBy: 'Bicep'
  }
}

// Module deployments
module identity 'modules/identity.bicep' = {
  scope: rg
  name: 'identity'
  params: {
    prefix: prefix
    location: location
  }
}

// ... other modules
```

## Parameter Files

```bicep
// parameters/dev.bicepparam
using '../main.bicep'

param environment = 'dev'
param location = 'westeurope'
param prefix = 'eshop'
```

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/main.bicep` |
| CREATE | `infra/bicepconfig.json` |
| CREATE | `infra/parameters/dev.bicepparam` |
| CREATE | `infra/parameters/prod.bicepparam` |

## Verification

```bash
# Validate Bicep syntax
az bicep build --file infra/main.bicep

# What-if deployment
az deployment sub what-if \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam
```

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 9.1 Infrastructure as Code)

---
## Notes
- Use Bicep instead of ARM templates for better readability
- All modules should have clearly defined inputs/outputs
