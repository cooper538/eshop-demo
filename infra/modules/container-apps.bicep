// Module: Container Apps
// Creates Container Apps Environment and applications
// Uses GitHub Container Registry (ghcr.io) for container images

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('User-Assigned Managed Identity resource ID')
param identityId string

@description('User-Assigned Managed Identity client ID')
param identityClientId string

@description('Log Analytics Workspace resource ID')
param logAnalyticsWorkspaceId string

@description('Key Vault URI')
param keyVaultUri string

@description('GitHub Container Registry username')
param ghcrUsername string

@description('GitHub Container Registry token (PAT)')
@secure()
param ghcrToken string

// App definitions
var apps = [
  { name: 'gateway', ingress: 'external', targetPort: 8080 }
  { name: 'product-service', ingress: 'internal', targetPort: 8080 }
  { name: 'order-service', ingress: 'internal', targetPort: 8080 }
  { name: 'notification-service', ingress: 'internal', targetPort: 8080 }
  { name: 'catalog-service', ingress: 'internal', targetPort: 8080 }
]

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: last(split(logAnalyticsWorkspaceId, '/'))
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-env'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
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

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = [
  for app in apps: {
    name: '${prefix}-${app.name}'
    location: location
    tags: tags
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
        secrets: [
          {
            name: 'ghcr-token'
            value: ghcrToken
          }
        ]
        registries: [
          {
            server: 'ghcr.io'
            username: ghcrUsername
            passwordSecretRef: 'ghcr-token'
          }
        ]
      }
      template: {
        containers: [
          {
            name: app.name
            // Placeholder image - will be updated by CI/CD to ghcr.io/USERNAME/eshop-SERVICE:TAG
            image: 'mcr.microsoft.com/dotnet/samples:aspnetapp'
            resources: {
              cpu: json('0.25')
              memory: '0.5Gi'
            }
            env: [
              { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
              { name: 'KeyVault__Uri', value: keyVaultUri }
              { name: 'KeyVault__ManagedIdentityClientId', value: identityClientId }
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
  }
]

@description('Container Apps Environment ID')
output environmentId string = environment.id

@description('Gateway FQDN')
output gatewayFqdn string = 'https://${containerApp[0].properties.configuration.ingress.fqdn}'
