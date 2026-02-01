# Phase 12: Azure App Adaptation

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Adapt application code to support Azure deployment alongside existing local Aspire development. Create environment-aware configuration that switches between local PostgreSQL and Azure PostgreSQL Flexible Server, and add Key Vault integration for secrets management.

## Scope
- [x] Add SSL mode handling for Azure PostgreSQL Flexible Server connections
- [x] Integrate Azure Key Vault configuration provider with DefaultAzureCredential
- [x] Configure gRPC clients for Container Apps service discovery (FQDN pattern)
- [x] Update all services to use environment-aware configuration

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | PostgreSQL SSL Configuration | ✅ completed | - |
| 2 | task-02 | Key Vault Integration | ✅ completed | - |
| 3 | task-03 | gRPC Service Discovery | ✅ completed | - |
| 4 | task-04 | Service Configuration Updates | ✅ completed | task-01, task-02, task-03 |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 11. Aspire Integration)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (MassTransit configuration - unchanged)

---
## Notes
- Messaging (RabbitMQ) unchanged - same config as local development
- Environment detection uses built-in `IsProduction()` / `IsDevelopment()`
- PostgreSQL SSL required for Azure Flexible Server
