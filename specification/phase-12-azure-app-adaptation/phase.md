# Phase 12: Azure App Adaptation

## Metadata
| Key | Value |
|-----|-------|
| Status | pending |

## Objective
Adapt application code to support Azure deployment alongside existing local Aspire development. Create environment-aware configuration that switches between local PostgreSQL and Azure PostgreSQL Flexible Server, and add Key Vault integration for secrets management.

> **Note:** Messaging (RabbitMQ) requires **zero code changes** - Azure uses RabbitMQ on ACI with the same
> MassTransit configuration as local development.

## Scope
- [ ] Add SSL mode handling for Azure PostgreSQL Flexible Server connections
- [ ] Integrate Azure Key Vault configuration provider with DefaultAzureCredential
- [ ] Configure gRPC clients for Container Apps service discovery (FQDN pattern)
- [ ] Add environment detection extensions (`IHostEnvironment.IsAzure()`)
- [ ] Update all services to use environment-aware configuration

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | PostgreSQL SSL Configuration | pending | - |
| 2 | task-02 | Key Vault Integration | pending | - |
| 3 | task-03 | gRPC Service Discovery | pending | - |
| 4 | task-04 | Environment Detection | pending | - |
| 5 | task-05 | Service Configuration Updates | pending | task-01, task-02, task-03, task-04 |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (MassTransit configuration - unchanged)

---
## Notes
- **Messaging unchanged:** RabbitMQ on Azure ACI uses same config as local development
- DefaultAzureCredential handles both local dev (Azure CLI) and Azure (Managed Identity)
- PostgreSQL SSL is required for Azure Flexible Server
