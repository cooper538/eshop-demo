# Task 01: Directory.Packages.props Update

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ⚪ pending |
| Dependencies | - |

## Objective
Verify and complete Aspire package references in Directory.Packages.props for centralized package management.

## Scope
- [ ] Verify Aspire hosting packages are present (Aspire.Hosting.PostgreSQL, Aspire.Hosting.RabbitMQ)
- [ ] Add Aspire component packages for services (Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Aspire.RabbitMQ.Client)
- [ ] Add Microsoft.Extensions.ServiceDiscovery package
- [ ] Add Microsoft.Extensions.Http.Resilience package
- [ ] Add OpenTelemetry packages (Exporter, Hosting, Instrumentation.*)

## Acceptance Criteria
- [ ] All Aspire packages have consistent version (9.x)
- [ ] OpenTelemetry packages are versioned consistently
- [ ] `dotnet restore` succeeds after changes
- [ ] No duplicate package version definitions

## Notes
- Current Aspire version in project: 9.2.0
- See spec: aspire-orchestration.md, Section 10 (Package Requirements)
- OpenTelemetry packages should be compatible with Aspire 9.x
