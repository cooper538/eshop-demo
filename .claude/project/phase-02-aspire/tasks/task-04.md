# Task 04: Service Integration Pattern

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |
| Dependencies | task-02, task-03 |
| Estimated Effort | S |

## Objective
Document the standard pattern for integrating services with Aspire and ServiceDefaults.

## Scope
- [ ] Create `docs/service-aspire-integration.md` with integration guide
- [ ] Document Program.cs pattern for services
- [ ] Document service registration in AppHost pattern
- [ ] Include code snippets for:
  - [ ] `AddServiceDefaults()` usage
  - [ ] `AddNpgsqlDbContext<T>()` usage
  - [ ] `AddRabbitMQClient()` usage
  - [ ] `MapDefaultEndpoints()` usage
  - [ ] Service discovery URL format

## Acceptance Criteria
- [ ] Document is clear and actionable
- [ ] Includes copy-paste ready code snippets
- [ ] References the specs for deeper details
- [ ] Future service phases can follow this pattern

## Notes
- See spec: aspire-orchestration.md, Section 5 (Service Integration)
- See spec: aspire-hybrid-configuration.md, Section 5 (Options Pattern)
- This enables consistent service implementation in phases 3-6
