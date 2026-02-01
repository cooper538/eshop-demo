// Module: Key Vault
// Creates Key Vault with connection string secrets

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Managed Identity principal ID')
param identityPrincipalId string

@description('PostgreSQL server FQDN')
param postgresServerFqdn string

@description('PostgreSQL admin login')
param postgresAdminLogin string

@description('PostgreSQL admin password')
@secure()
param postgresAdminPassword string

@description('RabbitMQ host IP')
param rabbitMqHost string

@description('RabbitMQ username')
param rabbitMqUser string

@description('RabbitMQ password')
@secure()
param rabbitMqPassword string

// TODO: Implement in task-04
// - Key Vault (Standard, RBAC)
// - Connection string secrets
// - Role assignment for managed identity

var databases = ['productdb', 'orderdb', 'notificationdb', 'catalogdb']

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${prefix}-kv'
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
  }
}

// Key Vault Secrets User role assignment
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, identityPrincipalId, 'Key Vault Secrets User')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// PostgreSQL connection strings
resource postgresSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for db in databases: {
    parent: keyVault
    name: 'ConnectionStrings--${db}'
    properties: {
      value: 'Host=${postgresServerFqdn};Database=${db};Username=${postgresAdminLogin};Password=${postgresAdminPassword};SslMode=Require'
    }
  }
]

// RabbitMQ connection string
resource rabbitMqSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--messaging'
  properties: {
    value: 'amqp://${rabbitMqUser}:${rabbitMqPassword}@${rabbitMqHost}:5672/'
  }
}

@description('Key Vault URI')
output vaultUri string = keyVault.properties.vaultUri

@description('Key Vault name')
output vaultName string = keyVault.name
