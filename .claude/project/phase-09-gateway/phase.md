# Phase 9: API Gateway

## Metadata
| Key | Value |
|-----|-------|
| Status | 🔵 in_progress |

## Objective
Configure YARP as single entry point

## Scope
- [ ] Create Gateway project
- [ ] Configure YARP reverse proxy
- [ ] Set up routing to Product and Order services
- [ ] Add rate limiting
- [ ] Add CorrelationId middleware (generation for external requests)

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 1 | [Project Setup](tasks/task-01-project-setup.md) | ✅ completed | - |
| 2 | [YARP Configuration](tasks/task-02-yarp-configuration.md) | ✅ completed | task-01 |
| 3 | [Rate Limiting](tasks/task-03-rate-limiting.md) | 🔵 in_progress | task-02 |
| 4 | [CorrelationId Integration](tasks/task-04-correlation-id.md) | ⚪ pending | task-02 |
| 5 | [AppHost Integration](tasks/task-05-apphost-integration.md) | ⚪ pending | task-01-04 |

## Related Specs
- → [README.md](../high-level-specs/README.md) (architecture section)
- → [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)
- → [internal-api-communication.md](../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
