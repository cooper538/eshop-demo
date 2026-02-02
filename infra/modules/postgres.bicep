// Module: PostgreSQL Flexible Server with VNet integration

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Administrator login')
param administratorLogin string

@description('Administrator password')
@secure()
param administratorPassword string

@description('Delegated subnet ID for PostgreSQL')
param delegatedSubnetId string

@description('Private DNS Zone ID for PostgreSQL')
param privateDnsZoneId string

var databases = [
  'productdb'
  'orderdb'
  'notificationdb'
]

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: '${prefix}-postgres'
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      delegatedSubnetResourceId: delegatedSubnetId
      privateDnsZoneArmResourceId: privateDnsZoneId
    }
  }
}

resource db 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = [
  for dbName in databases: {
    parent: postgresServer
    name: dbName
  }
]

@description('PostgreSQL server FQDN')
output serverFqdn string = postgresServer.properties.fullyQualifiedDomainName

@description('PostgreSQL server name')
output serverName string = postgresServer.name
