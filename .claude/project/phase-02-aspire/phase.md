# Phase 2: Aspire & ServiceDefaults

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Configure .NET Aspire for local development and service orchestration

## Scope
- [ ] Create `EShop.AppHost` (Aspire orchestrator)
- [ ] Create `EShop.ServiceDefaults` (shared configuration, health checks, OpenTelemetry)
- [ ] Configure PostgreSQL and RabbitMQ resources
- [ ] Set up service discovery

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Directory.Packages.props Update](./tasks/task-01.md) | :white_circle: pending | - |
| 02 | [ServiceDefaults Project](./tasks/task-02.md) | :white_circle: pending | task-01 |
| 03 | [AppHost Project](./tasks/task-03.md) | :white_circle: pending | task-01 |
| 04 | [Service Integration Pattern](./tasks/task-04.md) | :white_circle: pending | task-02, task-03 |

## Related Specs
- → [aspire-orchestration.md](../high-level-specs/aspire-orchestration.md)
- → [aspire-hybrid-configuration.md](../high-level-specs/aspire-hybrid-configuration.md)

---
## Notes
(Updated during implementation)
