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

// Web API services (with ingress + health probes)
var webApiApps = [
  { name: 'product-service', otelName: 'product-service' }
  { name: 'order-service', otelName: 'order-service' }
]

// Worker services (no ingress, no health probes)
var workerApps = [
  { name: 'notification-service', otelName: 'notification-service' }
  { name: 'analytics-service', otelName: 'analytics-service' }
]

// Common registry configuration
var registryConfig = {
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
      secrets: registryConfig.secrets
      registries: registryConfig.registries
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
            { name: 'AllowedHosts', value: '*.azurecontainerapps.io;localhost' }
            // Service discovery for YARP reverse proxy (internal Container Apps DNS)
            { name: 'services__product-service__http__0', value: 'http://${prefix}-product-service' }
            { name: 'services__order-service__http__0', value: 'http://${prefix}-order-service' }
            // Azure AD - disable auth when credentials are not provided
            { name: 'Gateway__Authentication__Enabled', value: empty(azureAdTenantId) ? 'false' : 'true' }
            { name: 'Gateway__Authentication__AzureAd__TenantId', value: azureAdTenantId }
            { name: 'Gateway__Authentication__AzureAd__ClientId', value: azureAdClientId }
          ])
          probes: [
            {
              type: 'Startup'
              httpGet: {
                path: '/alive'
                port: 8080
              }
              initialDelaySeconds: 5
              periodSeconds: 5
              failureThreshold: 12
            }
            {
              type: 'Liveness'
              httpGet: {
                path: '/alive'
                port: 8080
              }
              periodSeconds: 30
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
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

// Web API backend services (with ingress + health probes)
resource webApiService 'Microsoft.App/containerApps@2024-03-01' = [
  for app in webApiApps: {
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
        secrets: registryConfig.secrets
        registries: registryConfig.registries
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
              { name: 'AllowedHosts', value: '*' }
              // Order service needs product service URL for gRPC calls
              {
                name: 'ServiceClients__ProductService__Url'
                value: app.name == 'order-service' ? 'http://${prefix}-product-service:8080' : ''
              }
            ])
            probes: [
              {
                type: 'Startup'
                httpGet: {
                  path: '/alive'
                  port: 8080
                }
                initialDelaySeconds: 5
                periodSeconds: 5
                failureThreshold: 12
              }
              {
                type: 'Liveness'
                httpGet: {
                  path: '/alive'
                  port: 8080
                }
                periodSeconds: 30
                failureThreshold: 3
              }
            ]
          }
        ]
        scale: {
          minReplicas: 1
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

// Worker backend services (no ingress, no health probes)
resource workerService 'Microsoft.App/containerApps@2024-03-01' = [
  for app in workerApps: {
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
        secrets: registryConfig.secrets
        registries: registryConfig.registries
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
              { name: 'AllowedHosts', value: '*' }
            ])
          }
        ]
        scale: {
          minReplicas: 1
          maxReplicas: 2
        }
      }
    }
  }
]

// Database migration job
resource migrationJob 'Microsoft.App/jobs@2024-03-01' = {
  name: '${prefix}-migration'
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
      triggerType: 'Manual'
      replicaTimeout: 300
      replicaRetryLimit: 1
      secrets: registryConfig.secrets
      registries: registryConfig.registries
    }
    template: {
      containers: [
        {
          name: 'migration'
          image: 'mcr.microsoft.com/dotnet/samples:aspnetapp'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: commonEnv
        }
      ]
    }
  }
}

output gatewayFqdn string = 'https://${gateway.properties.configuration.ingress.fqdn}'
