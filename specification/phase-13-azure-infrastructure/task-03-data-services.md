# Task 03: Data Services Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Create Bicep modules for PostgreSQL Flexible Server with multiple databases and RabbitMQ on Azure Container Instance for messaging.

## Scope
- [x] Create `postgres.bicep` module with Flexible Server B1ms tier
- [x] Configure 4 databases: productdb, orderdb, notificationdb, catalogdb
- [x] Add firewall rules for Azure services access
- [x] Create `rabbitmq.bicep` module with Azure Container Instance
- [x] Output connection strings for Key Vault storage

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources, 4. Messaging Resources)

---
## Notes
- PostgreSQL B1ms: ~$12/month running, ~$3.68/month stopped (storage only)
- RabbitMQ ACI (0.5 vCPU, 1GB): ~$10/month running, $0 stopped
- RabbitMQ uses same MassTransit config as local development (no code changes)
- RabbitMQ is ephemeral (no persistent storage) - simplified for demo purposes
