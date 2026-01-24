# Task 01: Directory.Packages.props Update

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Objective
Verify and complete Aspire package references in Directory.Packages.props for centralized package management.

## Scope
- [x] Verify Aspire hosting packages are present (Aspire.Hosting.PostgreSQL, Aspire.Hosting.RabbitMQ)
- [x] Add Aspire component packages for services (Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Aspire.RabbitMQ.Client)
- [x] Add Microsoft.Extensions.ServiceDiscovery package
- [x] Add Microsoft.Extensions.Http.Resilience package
- [x] Add OpenTelemetry packages (Exporter, Hosting, Instrumentation.*)

## Acceptance Criteria
- [x] All Aspire packages have consistent version (9.x)
- [x] OpenTelemetry packages are versioned consistently
- [x] `dotnet restore` succeeds after changes
- [x] No duplicate package version definitions

## Notes
- Current Aspire version in project: 9.2.0
- See spec: aspire-orchestration.md, Section 10 (Package Requirements)
- OpenTelemetry packages should be compatible with Aspire 9.x
