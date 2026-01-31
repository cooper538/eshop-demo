# Phase 12: Azure App Adaptation

## Metadata
| Key | Value |
|-----|-------|
| Status | pending |

## Objective
Adapt application code to support Azure deployment alongside existing local Aspire development. Create environment-aware configuration that switches between RabbitMQ/PostgreSQL (local) and Azure Service Bus/PostgreSQL Flexible Server (cloud).

## Scope
- [ ] Create `AddMessagingAzure<T>()` extension for Azure Service Bus with MassTransit
- [ ] Configure MassTransit endpoint conventions for Basic tier (queues only, no topics)
- [ ] Add SSL mode handling for Azure PostgreSQL Flexible Server connections
- [ ] Integrate Azure Key Vault configuration provider with DefaultAzureCredential
- [ ] Configure gRPC clients for Container Apps service discovery (FQDN pattern)
- [ ] Add environment detection extensions (`IHostEnvironment.IsAzure()`)
- [ ] Update all services to use environment-aware configuration

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | Service Bus MassTransit Configuration | pending | - |
| 2 | task-02 | Endpoint Conventions for Basic Tier | pending | task-01 |
| 3 | task-03 | PostgreSQL SSL Configuration | pending | - |
| 4 | task-04 | Key Vault Integration | pending | - |
| 5 | task-05 | gRPC Service Discovery | pending | - |
| 6 | task-06 | Environment Detection | pending | - |
| 7 | task-07 | Service Configuration Updates | pending | task-01, task-02, task-03, task-04, task-05, task-06 |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 4. Messaging Resources, 11. Aspire Integration)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (MassTransit configuration)

---
## Notes
- Azure Service Bus Basic tier supports only queues (no topics/subscriptions)
- MassTransit requires endpoint conventions to simulate pub/sub with queues
- DefaultAzureCredential handles both local dev (Azure CLI) and Azure (Managed Identity)
