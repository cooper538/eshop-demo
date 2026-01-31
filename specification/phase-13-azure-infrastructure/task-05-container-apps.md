# Task 05: Container Apps Module

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | pending |
| Dependencies | task-02, task-03, task-04 |

## Summary
Create Bicep module for Container Apps Environment and 6 Container Apps with scale-to-zero configuration, internal/external ingress, and managed identity assignment.

## Scope
- [ ] Create `acr.bicep` module for Container Registry (Basic tier)
- [ ] Create `container-apps.bicep` module with Container Apps Environment
- [ ] Configure Consumption workload profile (serverless)
- [ ] Create 6 Container Apps: gateway, product-service, order-service, notification-service, catalog-service, database-migration
- [ ] Configure scale-to-zero (min replicas: 0, max: 2)
- [ ] Assign User-Assigned Managed Identity to all apps
- [ ] Configure internal ingress for services, external for gateway
- [ ] Set up environment variables for Key Vault URI and service URLs
- [ ] Configure Log Analytics integration

## Container Registry Module

```bicep
// modules/acr.bicep
param prefix string
param location string
param identityPrincipalId string

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: '${replace(prefix, '-', '')}acr'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

// Grant AcrPull to managed identity
resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: acr
  name: guid(acr.id, identityPrincipalId, 'AcrPull')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output acrLoginServer string = acr.properties.loginServer
output acrName string = acr.name
```

## Container Apps Module

```bicep
// modules/container-apps.bicep
param prefix string
param location string
param logAnalyticsWorkspaceId string
param logAnalyticsCustomerId string
@secure()
param logAnalyticsSharedKey string
param identityId string
param acrLoginServer string
param keyVaultUri string

// Container Apps Environment
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

// App definitions
var apps = [
  { name: 'gateway', ingress: 'external', targetPort: 8080 }
  { name: 'product-service', ingress: 'internal', targetPort: 8080 }
  { name: 'order-service', ingress: 'internal', targetPort: 8080 }
  { name: 'notification-service', ingress: 'internal', targetPort: 8080 }
  { name: 'catalog-service', ingress: 'internal', targetPort: 8080 }
]

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = [for app in apps: {
  name: '${prefix}-${app.name}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    environmentId: environment.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: app.ingress == 'external'
        targetPort: app.targetPort
        transport: 'http'
      }
      registries: [
        {
          server: acrLoginServer
          identity: identityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: app.name
          image: 'mcr.microsoft.com/dotnet/samples:aspnetapp' // Placeholder
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'KeyVault__Uri', value: keyVaultUri }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}]

output environmentId string = environment.id
output gatewayFqdn string = containerApp[0].properties.configuration.ingress.fqdn
```

## Container Apps Configuration

| App | Ingress | vCPU | Memory | Min/Max Replicas |
|-----|---------|------|--------|------------------|
| gateway | External | 0.25 | 0.5Gi | 0/2 |
| product-service | Internal | 0.25 | 0.5Gi | 0/2 |
| order-service | Internal | 0.25 | 0.5Gi | 0/2 |
| notification-service | Internal | 0.25 | 0.5Gi | 0/2 |
| catalog-service | Internal | 0.25 | 0.5Gi | 0/2 |

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/modules/acr.bicep` |
| CREATE | `infra/modules/container-apps.bicep` |
| MODIFY | `infra/main.bicep` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 2. Compute Resources, 6. Container Registry)

---
## Notes
- Placeholder image used for initial deployment; real images deployed via CI/CD
- Scale-to-zero provides cost savings when idle
- External ingress on gateway only; all services internal
