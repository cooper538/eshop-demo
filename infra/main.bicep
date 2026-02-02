// EShop Demo - Azure Infrastructure
targetScope = 'subscription'

// Parameters
@description('Environment suffix')
param environment string = 'prod'

@description('Azure region')
param location string = 'westeurope'

@minLength(2)
@maxLength(10)
param prefix string = 'eshop'

param postgresAdminLogin string = 'eshop_admin'

@secure()
param postgresAdminPassword string

param rabbitMqUser string = 'eshop'

@secure()
param rabbitMqPassword string

param ghcrUsername string

@secure()
param ghcrToken string

@description('Azure AD Tenant ID for JWT authentication (optional)')
param azureAdTenantId string = ''

@description('Azure AD Client ID for JWT authentication (optional)')
param azureAdClientId string = ''

// Variables
var resourceGroupName = '${prefix}-${environment}-rg'
var tags = {
  Environment: environment
  Project: 'EShop Demo'
  ManagedBy: 'Bicep'
}

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Networking (VNet + Subnets + NSG)
module networking 'modules/networking.bicep' = {
  scope: rg
  name: 'networking-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
  }
}

// Identity (Managed Identity)
module identity 'modules/identity.bicep' = {
  scope: rg
  name: 'identity-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
  }
}

// Monitoring (Log Analytics)
module monitoring 'modules/monitoring.bicep' = {
  scope: rg
  name: 'monitoring-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
  }
}

// PostgreSQL with VNet integration
module postgres 'modules/postgres.bicep' = {
  scope: rg
  name: 'postgres-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    administratorLogin: postgresAdminLogin
    administratorPassword: postgresAdminPassword
    delegatedSubnetId: networking.outputs.postgresSubnetId
    privateDnsZoneId: networking.outputs.postgresDnsZoneId
  }
}

// Container Apps Environment with VNet
module containerAppsEnv 'modules/container-apps-env.bicep' = {
  scope: rg
  name: 'containerapps-env-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.workspaceId
    subnetId: networking.outputs.containerAppsSubnetId
  }
}

// RabbitMQ (internal only)
module rabbitmq 'modules/rabbitmq.bicep' = {
  scope: rg
  name: 'rabbitmq-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    environmentId: containerAppsEnv.outputs.environmentId
    rabbitMqUser: rabbitMqUser
    rabbitMqPassword: rabbitMqPassword
  }
}

// Key Vault
module keyVault 'modules/key-vault.bicep' = {
  scope: rg
  name: 'keyvault-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    identityPrincipalId: identity.outputs.identityPrincipalId
    postgresServerFqdn: postgres.outputs.serverFqdn
    postgresAdminLogin: postgresAdminLogin
    postgresAdminPassword: postgresAdminPassword
    rabbitMqHost: rabbitmq.outputs.rabbitMqHost
    rabbitMqUser: rabbitMqUser
    rabbitMqPassword: rabbitMqPassword
  }
}

// Container Apps
module containerApps 'modules/container-apps.bicep' = {
  scope: rg
  name: 'containerapps-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    identityId: identity.outputs.identityId
    identityClientId: identity.outputs.identityClientId
    environmentId: containerAppsEnv.outputs.environmentId
    keyVaultUri: keyVault.outputs.vaultUri
    ghcrUsername: ghcrUsername
    ghcrToken: ghcrToken
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    azureAdTenantId: azureAdTenantId
    azureAdClientId: azureAdClientId
  }
}

// Outputs
output resourceGroupName string = rg.name
output gatewayUrl string = containerApps.outputs.gatewayFqdn
output keyVaultUri string = keyVault.outputs.vaultUri
