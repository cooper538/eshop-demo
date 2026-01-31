# Task 02: Identity and Monitoring Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | pending |
| Dependencies | task-01 |

## Summary
Create Bicep modules for User-Assigned Managed Identity with RBAC role assignments and Log Analytics Workspace for centralized logging.

## Scope
- [ ] Create `identity.bicep` module with User-Assigned Managed Identity
- [ ] Configure role assignments for all Azure resources (Key Vault, PostgreSQL, Service Bus, ACR)
- [ ] Create `monitoring.bicep` module with Log Analytics Workspace
- [ ] Configure 30-day retention and pay-as-you-go tier
- [ ] Output identity principal ID and workspace ID for use by other modules
- [ ] Add diagnostic settings outputs for container apps logging

## Identity Module

```bicep
// modules/identity.bicep
param prefix string
param location string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-identity'
  location: location
}

// Role definitions (built-in)
var keyVaultSecretsUserRole = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User

var serviceBusDataOwnerRole = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '090c5cfd-751d-490a-894a-3ce6f1109419') // Service Bus Data Owner

var acrPullRole = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull

output identityId string = managedIdentity.id
output identityPrincipalId string = managedIdentity.properties.principalId
output identityClientId string = managedIdentity.properties.clientId
```

## RBAC Role Assignments

| Role | Resource | Purpose |
|------|----------|---------|
| Key Vault Secrets User | Key Vault | Read connection strings |
| Azure Service Bus Data Owner | Service Bus | Send/receive messages |
| AcrPull | Container Registry | Pull container images |

## Monitoring Module

```bicep
// modules/monitoring.bicep
param prefix string
param location string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018' // Pay-as-you-go
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

output workspaceId string = logAnalytics.id
output workspaceCustomerId string = logAnalytics.properties.customerId
```

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/modules/identity.bicep` |
| CREATE | `infra/modules/monitoring.bicep` |
| MODIFY | `infra/main.bicep` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.1 User-Assigned Managed Identity, 7. Monitoring)

---
## Notes
- Single managed identity shared by all Container Apps reduces complexity
- Role assignments are created at resource scope (not subscription)
- Log Analytics 5 GB/month free ingestion covers demo usage
