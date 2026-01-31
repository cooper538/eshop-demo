# Task 04: Key Vault Module

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | pending |
| Dependencies | task-03 |

## Summary
Create Bicep module for Azure Key Vault with connection string secrets for PostgreSQL and RabbitMQ.

## Scope
- [ ] Create `key-vault.bicep` module with Standard SKU
- [ ] Configure RBAC authorization mode (not access policies)
- [ ] Store PostgreSQL connection strings for all databases
- [ ] Store RabbitMQ connection string
- [ ] Grant Key Vault Secrets User role to managed identity
- [ ] Enable soft delete with 7-day retention
- [ ] Disable purge protection for demo cleanup capability

## Key Vault Module

```bicep
// modules/key-vault.bicep
param prefix string
param location string
param identityPrincipalId string

// PostgreSQL parameters for connection strings
param postgresServerFqdn string
param postgresAdminLogin string
@secure()
param postgresAdminPassword string

// RabbitMQ parameters
param rabbitMqIp string
@secure()
param rabbitMqUser string
@secure()
param rabbitMqPassword string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${prefix}-kv'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false // Allow cleanup for demo
  }
}

// Grant Secrets User role to managed identity
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, identityPrincipalId, 'Key Vault Secrets User')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// PostgreSQL connection string secrets
var databases = ['productdb', 'orderdb', 'notificationdb', 'catalogdb']

resource postgresSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [for db in databases: {
  parent: keyVault
  name: 'ConnectionStrings--${db}'
  properties: {
    value: 'Host=${postgresServerFqdn};Database=${db};Username=${postgresAdminLogin};Password=${postgresAdminPassword};SslMode=Require'
  }
}]

// RabbitMQ connection string
resource rabbitMqSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--messaging'
  properties: {
    value: 'amqp://${rabbitMqUser}:${rabbitMqPassword}@${rabbitMqIp}:5672/'
  }
}

// RabbitMQ credentials (separate secrets for management)
resource rabbitMqUserSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'RabbitMQ--User'
  properties: {
    value: rabbitMqUser
  }
}

resource rabbitMqPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'RabbitMQ--Password'
  properties: {
    value: rabbitMqPassword
  }
}

output vaultUri string = keyVault.properties.vaultUri
output vaultName string = keyVault.name
```

## Secrets Stored

| Secret Name | Description |
|-------------|-------------|
| ConnectionStrings--productdb | Product database connection string |
| ConnectionStrings--orderdb | Order database connection string |
| ConnectionStrings--notificationdb | Notification database connection string |
| ConnectionStrings--catalogdb | Catalog database connection string |
| ConnectionStrings--messaging | RabbitMQ AMQP connection string |
| RabbitMQ--User | RabbitMQ admin username |
| RabbitMQ--Password | RabbitMQ admin password |

## Secret Name Convention

Key Vault secret names use `--` as separator (translated to `:` by configuration provider):
- `ConnectionStrings--productdb` -> `ConnectionStrings:productdb`
- `ConnectionStrings--messaging` -> `ConnectionStrings:messaging`

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/modules/key-vault.bicep` |
| MODIFY | `infra/main.bicep` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.2 Key Vault)

---
## Notes
- RBAC authorization is preferred over access policies
- Soft delete enabled but purge protection disabled for easy cleanup
- Secret names follow configuration key convention with `--` separator
- RabbitMQ connection string uses same `messaging` key as local Aspire setup
