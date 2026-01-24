# Task 04: Service Integration Pattern

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-02, task-03 |

## Objective
Document the standard pattern for integrating services with Aspire and ServiceDefaults.

## Scope
- [x] Create `docs/service-aspire-integration.md` with integration guide
- [x] Document Program.cs pattern for services
- [x] Document service registration in AppHost pattern
- [x] Include code snippets for:
  - [x] `AddServiceDefaults()` usage
  - [x] `AddNpgsqlDbContext<T>()` usage
  - [x] `AddRabbitMQClient()` usage
  - [x] `MapDefaultEndpoints()` usage
  - [x] Service discovery URL format

## Acceptance Criteria
- [x] Document is clear and actionable
- [x] Includes copy-paste ready code snippets
- [x] References the specs for deeper details
- [x] Future service phases can follow this pattern

## Notes
- See spec: aspire-orchestration.md, Section 5 (Service Integration)
- See spec: aspire-hybrid-configuration.md, Section 5 (Options Pattern)
- This enables consistent service implementation in phases 3-6
