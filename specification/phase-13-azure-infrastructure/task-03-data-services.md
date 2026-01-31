# Task 03: Data Services Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | pending |
| Dependencies | task-02 |

## Summary
Create Bicep modules for PostgreSQL Flexible Server with multiple databases and Azure Service Bus with queues for messaging.

## Scope
- [ ] Create `postgres.bicep` module with Flexible Server B1ms tier
- [ ] Configure 4 databases: productdb, orderdb, notificationdb, catalogdb
- [ ] Enable Microsoft Entra authentication for PostgreSQL
- [ ] Add firewall rules for Azure services access
- [ ] Create `service-bus.bicep` module with Basic tier namespace
- [ ] Create 5 queues: order-confirmed, order-rejected, order-cancelled, stock-low, stock-reservation-expired
- [ ] Output connection strings for Key Vault storage

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

## Service Bus Module

```bicep
// modules/service-bus.bicep
param prefix string
param location string

var queues = [
  'order-confirmed'
  'order-rejected'
  'order-cancelled'
  'stock-low'
  'stock-reservation-expired'
]

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: '${prefix}-servicebus'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = [for queueName in queues: {
  parent: serviceBus
  name: queueName
  properties: {
    lockDuration: 'PT1M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
  }
}]

output namespaceName string = serviceBus.name
output namespaceEndpoint string = serviceBus.properties.serviceBusEndpoint
```

## Database Configuration

| Database | Service | Tables |
|----------|---------|--------|
| productdb | Product Service | Products, Categories, Stocks, StockReservations |
| orderdb | Order Service | Orders, OrderItems, OutboxMessages |
| notificationdb | Notification Service | ProcessedMessages (Inbox) |
| catalogdb | Catalog Service | SearchableProducts |

## Files to Create

| Action | File |
|--------|------|
| CREATE | `infra/modules/postgres.bicep` |
| CREATE | `infra/modules/service-bus.bicep` |
| MODIFY | `infra/main.bicep` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources, 4. Messaging Resources)

---
## Notes
- PostgreSQL B1ms: ~$12/month running, ~$3.68/month stopped (storage only)
- Service Bus Basic: ~$0.05/month (no topics, queues only)
- All databases on single server to minimize cost
