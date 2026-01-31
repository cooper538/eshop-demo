# Task 03: Data Services Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | pending |
| Dependencies | task-02 |

## Summary
Create Bicep modules for PostgreSQL Flexible Server with multiple databases and RabbitMQ on Azure Container Instance for messaging.

## Scope
- [ ] Create `storage.bicep` module with Storage Account and File Share (for RabbitMQ persistence)
- [ ] Create `postgres.bicep` module with Flexible Server B1ms tier
- [ ] Configure 4 databases: productdb, orderdb, notificationdb, catalogdb
- [ ] Enable Microsoft Entra authentication for PostgreSQL
- [ ] Add firewall rules for Azure services access
- [ ] Create `rabbitmq.bicep` module with Azure Container Instance
- [ ] Configure persistent storage via Azure File Share
- [ ] Output connection strings for Key Vault storage

## Storage Module (for RabbitMQ)

```bicep
// modules/storage.bicep
param prefix string
param location string

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: '${replace(prefix, '-', '')}storage'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource rabbitMqShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: fileService
  name: 'rabbitmq-data'
  properties: {
    shareQuota: 1 // 1 GB
  }
}

output storageAccountName string = storageAccount.name
output storageAccountKey string = storageAccount.listKeys().keys[0].value
output fileShareName string = rabbitMqShare.name
```

## PostgreSQL Module

```bicep
// modules/postgres.bicep
param prefix string
param location string
param administratorLogin string = 'eshop_admin'
@secure()
param administratorPassword string

var databases = [
  'productdb'
  'orderdb'
  'notificationdb'
  'catalogdb'
]

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: '${prefix}-postgres'
  location: location
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
  }
}

// Allow Azure services
resource firewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Create databases
resource db 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = [for dbName in databases: {
  parent: postgresServer
  name: dbName
}]

output serverFqdn string = postgresServer.properties.fullyQualifiedDomainName
output serverName string = postgresServer.name
```

## RabbitMQ Module (Azure Container Instance)

```bicep
// modules/rabbitmq.bicep
param prefix string
param location string
@secure()
param rabbitMqUser string
@secure()
param rabbitMqPassword string
param storageAccountName string
@secure()
param storageAccountKey string
param fileShareName string

resource rabbitMq 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: '${prefix}-rabbitmq'
  location: location
  properties: {
    containers: [
      {
        name: 'rabbitmq'
        properties: {
          image: 'rabbitmq:3-management-alpine'
          resources: {
            requests: {
              cpu: json('0.5')
              memoryInGB: json('1.0')
            }
          }
          ports: [
            { port: 5672, protocol: 'TCP' }   // AMQP
            { port: 15672, protocol: 'TCP' }  // Management UI
          ]
          environmentVariables: [
            { name: 'RABBITMQ_DEFAULT_USER', value: rabbitMqUser }
            { name: 'RABBITMQ_DEFAULT_PASS', secureValue: rabbitMqPassword }
          ]
          volumeMounts: [
            {
              name: 'rabbitmq-data'
              mountPath: '/var/lib/rabbitmq'
            }
          ]
        }
      }
    ]
    osType: 'Linux'
    restartPolicy: 'Always'
    ipAddress: {
      type: 'Public'
      ports: [
        { port: 5672, protocol: 'TCP' }
        { port: 15672, protocol: 'TCP' }
      ]
    }
    volumes: [
      {
        name: 'rabbitmq-data'
        azureFile: {
          shareName: fileShareName
          storageAccountName: storageAccountName
          storageAccountKey: storageAccountKey
        }
      }
    ]
  }
}

output rabbitMqFqdn string = rabbitMq.properties.ipAddress.fqdn
output rabbitMqIp string = rabbitMq.properties.ipAddress.ip
```

## Database Configuration

| Database | Service | Tables |
|----------|---------|--------|
| productdb | Product Service | Products, Categories, Stocks, StockReservations |
| orderdb | Order Service | Orders, OrderItems, OutboxMessages |
| notificationdb | Notification Service | ProcessedMessages (Inbox) |
| catalogdb | Catalog Service | SearchableProducts |

## Connection String Format

| Service | Format |
|---------|--------|
| PostgreSQL | `Host={fqdn};Database={db};Username={user};Password={pass};SslMode=Require` |
| RabbitMQ | `amqp://{user}:{pass}@{ip}:5672/` |

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/modules/storage.bicep` |
| CREATE | `infra/modules/postgres.bicep` |
| CREATE | `infra/modules/rabbitmq.bicep` |
| MODIFY | `infra/main.bicep` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources, 4. Messaging Resources)

---
## Notes
- PostgreSQL B1ms: ~$12/month running, ~$3.68/month stopped (storage only)
- RabbitMQ ACI (0.5 vCPU, 1GB): ~$10/month running, $0 stopped
- Storage Account (1GB file share): ~$0.10/month
- All databases on single server to minimize cost
- RabbitMQ uses same MassTransit config as local development (no code changes)
