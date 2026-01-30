# Phase 2: Aspire & ServiceDefaults

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Configure .NET Aspire for local development and service orchestration

## Scope
- [x] Create `EShop.AppHost` (Aspire orchestrator)
- [x] Create `EShop.ServiceDefaults` (shared configuration, health checks, OpenTelemetry)
- [x] Configure PostgreSQL (3 databases) and RabbitMQ resources
- [x] Set up service discovery and resilience
- [x] Configure Serilog with CorrelationId support
- [x] Add Docker Compose publishing support

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Directory.Packages.props Update](./tasks/task-01.md) | ✅ completed | - |
| 02 | [ServiceDefaults Project](./tasks/task-02.md) | ✅ completed | task-01 |
| 03 | [AppHost Project](./tasks/task-03.md) | ✅ completed | task-01 |
| 04 | [Service Integration Pattern](./tasks/task-04.md) | ✅ completed | task-02, task-03 |

## Related Specs
- → [aspire-orchestration.md](../high-level-specs/aspire-orchestration.md)
- → [aspire-hybrid-configuration.md](../high-level-specs/aspire-hybrid-configuration.md)

---
## Notes
- Aspire SDK version: 13.1.0
- Full orchestration of all services: Product, Order, Notification, Analytics, Gateway, Migration
- Docker Compose publishing configured for production deployments
