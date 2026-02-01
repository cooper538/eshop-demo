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
param appInsightsConnectionString string

@description('Azure AD Tenant ID for JWT authentication')
param azureAdTenantId string = ''

@description('Azure AD Client ID for JWT authentication')
param azureAdClientId string = ''

@secure()
param ghcrToken string

// Common environment variables for all services
var commonEnv = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'KeyVault__Uri', value: keyVaultUri }
  { name: 'KeyVault__ManagedIdentityClientId', value: identityClientId }
  { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
  { name: 'OTEL_EXPORTER_OTLP_PROTOCOL', value: 'grpc' }
]

// Backend services (internal)
var backendApps = [
  { name: 'product-service', otelName: 'product-service' }
  { name: 'order-service', otelName: 'order-service' }
  { name: 'notification-service', otelName: 'notification-service' }
  { name: 'analytics-service', otelName: 'analytics-service' }
]

// Gateway (external) - deployed first so we can reference backend FQDNs
resource gateway 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${prefix}-gateway'
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
        external: true
        targetPort: 8080
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
          name: 'gateway'
          image: 'mcr.microsoft.com/dotnet/samples:aspnetapp'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: union(commonEnv, [
            { name: 'OTEL_SERVICE_NAME', value: 'gateway' }
            // Service discovery for YARP reverse proxy (internal Container Apps DNS)
            { name: 'services__product-service__http__0', value: 'http://${prefix}-product-service' }
            { name: 'services__order-service__http__0', value: 'http://${prefix}-order-service' }
            // Azure AD configuration (only if provided)
            { name: 'Gateway__Authentication__AzureAd__TenantId', value: azureAdTenantId }
            { name: 'Gateway__Authentication__AzureAd__ClientId', value: azureAdClientId }
          ])
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

// Backend services
resource backendService 'Microsoft.App/containerApps@2024-03-01' = [
  for app in backendApps: {
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
          external: false
          targetPort: 8080
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
            env: union(commonEnv, [
              { name: 'OTEL_SERVICE_NAME', value: app.otelName }
              // Order service needs product service URL for gRPC calls
              {
                name: 'services__product-service__http__0'
                value: app.name == 'order-service' ? 'http://${prefix}-product-service' : ''
              }
            ])
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

output gatewayFqdn string = 'https://${gateway.properties.configuration.ingress.fqdn}'
