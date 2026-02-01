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
- [ ] Add firewall rules for Azure services access
- [ ] Create `rabbitmq.bicep` module with Azure Container Instance
- [ ] Configure persistent storage via Azure File Share
- [ ] Output connection strings for Key Vault storage

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources, 4. Messaging Resources)

---
## Notes
- PostgreSQL B1ms: ~$12/month running, ~$3.68/month stopped (storage only)
- RabbitMQ ACI (0.5 vCPU, 1GB): ~$10/month running, $0 stopped
- RabbitMQ uses same MassTransit config as local development (no code changes)
