# Task 03: Data Services Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Create Bicep modules for PostgreSQL Flexible Server with multiple databases and RabbitMQ as internal Container App for messaging.

## Scope
- [x] Create `postgres.bicep` module with Flexible Server B1ms tier
- [x] Configure 3 databases: productdb, orderdb, notificationdb
- [x] Add firewall rules for Azure services access
- [x] Create `rabbitmq.bicep` module as internal Container App (same environment)
- [x] Output connection strings for Key Vault storage

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 3. Data Resources, 4. Messaging Resources)

---
## Notes
- PostgreSQL B1ms: ~$12/month running, ~$3.68/month stopped (storage only)
- RabbitMQ runs as internal Container App (0.5 vCPU, 1GB) - simpler networking than ACI
- RabbitMQ uses same MassTransit config as local development (no code changes)
- RabbitMQ is ephemeral (no persistent storage) - queues/messages lost on restart
- **Demo limitation**: For production, add Azure Files volume for RabbitMQ persistence
