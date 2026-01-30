# Phase 9: API Gateway

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Configure YARP as single entry point

## Scope
- [x] Create Gateway project
- [x] Configure YARP reverse proxy
- [x] Set up routing to Product and Order services
- [x] Add rate limiting
- [x] Add CorrelationId middleware (generation for external requests)

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 1 | [Project Setup](tasks/task-01-project-setup.md) | ✅ completed | - |
| 2 | [YARP Configuration](tasks/task-02-yarp-configuration.md) | ✅ completed | task-01 |
| 3 | [Rate Limiting](tasks/task-03-rate-limiting.md) | ✅ completed | task-02 |
| 4 | [CorrelationId Integration](tasks/task-04-correlation-id.md) | ✅ completed | task-02 |
| 5 | [AppHost Integration](tasks/task-05-apphost-integration.md) | ✅ completed | task-01-04 |

## Related Specs
- → [README.md](../high-level-specs/README.md) (architecture section)
- → [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)
- → [internal-api-communication.md](../high-level-specs/internal-api-communication.md)

---
## Notes
- Project named `Gateway.API` (not EShop.Gateway.Api as originally planned)
- YAML configuration (`gateway.settings.yaml`) instead of appsettings.json
- Output caching added for GET requests (products list 5min, detail 2min, swagger 24h)
- Health checks for downstream services via Aspire service discovery
- Rate limiting with ProblemDetails response (not X-RateLimit-* headers)
