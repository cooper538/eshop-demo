// Module: Container Apps

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

param identityId string
param identityClientId string
param environmentId string
param keyVaultUri string
param ghcrUsername string

@secure()
param ghcrToken string

var apps = [
  { name: 'gateway', ingress: 'external', targetPort: 8080 }
  { name: 'product-service', ingress: 'internal', targetPort: 8080 }
  { name: 'order-service', ingress: 'internal', targetPort: 8080 }
  { name: 'notification-service', ingress: 'internal', targetPort: 8080 }
  { name: 'analytics-service', ingress: 'internal', targetPort: 8080 }
]

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
      environmentId: environmentId
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

output gatewayFqdn string = 'https://${containerApp[0].properties.configuration.ingress.fqdn}'
