// EShop Demo - Azure Infrastructure
// Entry point for infrastructure deployment
targetScope = 'subscription'

// ============================================================================
// Parameters
// ============================================================================

@description('Environment suffix for resource naming')
param environment string = 'prod'

@description('Azure region for all resources')
param location string = 'westeurope'

@description('Resource naming prefix')
@minLength(2)
@maxLength(10)
param prefix string = 'eshop'

@description('PostgreSQL administrator login')
param postgresAdminLogin string = 'eshop_admin'

@description('PostgreSQL administrator password')
@secure()
param postgresAdminPassword string

@description('RabbitMQ default username')
param rabbitMqUser string = 'eshop'

@description('RabbitMQ default password')
@secure()
param rabbitMqPassword string

@description('GitHub Container Registry username (GitHub username)')
param ghcrUsername string

@description('GitHub Container Registry token (PAT with read:packages scope)')
@secure()
param ghcrToken string

// ============================================================================
// Variables
// ============================================================================

var resourceGroupName = '${prefix}-${environment}-rg'

var tags = {
  Environment: environment
  Project: 'EShop Demo'
  ManagedBy: 'Bicep'
}

// ============================================================================
// Resource Group
// ============================================================================

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// ============================================================================
// Modules
// ============================================================================

// Module: Identity (User-Assigned Managed Identity + Role Assignments)
module identity 'modules/identity.bicep' = {
  scope: rg
  name: 'identity-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
  }
}

// Module: Monitoring (Log Analytics Workspace)
module monitoring 'modules/monitoring.bicep' = {
  scope: rg
  name: 'monitoring-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
  }
}

// Module: PostgreSQL (Flexible Server + Databases)
module postgres 'modules/postgres.bicep' = {
  scope: rg
  name: 'postgres-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    administratorLogin: postgresAdminLogin
    administratorPassword: postgresAdminPassword
  }
}

// Module: RabbitMQ (Azure Container Instance)
module rabbitmq 'modules/rabbitmq.bicep' = {
  scope: rg
  name: 'rabbitmq-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    rabbitMqUser: rabbitMqUser
    rabbitMqPassword: rabbitMqPassword
  }
}

// Module: Key Vault (Secrets storage)
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
    rabbitMqHost: rabbitmq.outputs.rabbitMqIp
    rabbitMqUser: rabbitMqUser
    rabbitMqPassword: rabbitMqPassword
  }
}

// Module: Container Apps (Environment + Apps)
// Uses GitHub Container Registry (ghcr.io)
module containerApps 'modules/container-apps.bicep' = {
  scope: rg
  name: 'containerapps-${uniqueString(rg.id)}'
  params: {
    prefix: prefix
    location: location
    tags: tags
    identityId: identity.outputs.identityId
    identityClientId: identity.outputs.identityClientId
    logAnalyticsWorkspaceId: monitoring.outputs.workspaceId
    keyVaultUri: keyVault.outputs.vaultUri
    ghcrUsername: ghcrUsername
    ghcrToken: ghcrToken
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('Resource group name')
output resourceGroupName string = rg.name

@description('Gateway public URL')
output gatewayUrl string = containerApps.outputs.gatewayFqdn

@description('Key Vault URI')
output keyVaultUri string = keyVault.outputs.vaultUri
