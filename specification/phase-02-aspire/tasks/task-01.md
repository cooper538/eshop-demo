# Task 01: Directory.Packages.props Update

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Objective
Configure Aspire and related package references in Directory.Packages.props for centralized package management.

## Scope
- [x] Add Aspire hosting packages (Aspire.Hosting.AppHost, Aspire.Hosting.PostgreSQL, Aspire.Hosting.RabbitMQ)
- [x] Add Aspire.Hosting.Docker for Docker Compose publishing
- [x] Add Aspire component packages (Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Aspire.RabbitMQ.Client)
- [x] Add Microsoft.Extensions.ServiceDiscovery package
- [x] Add OpenTelemetry packages (Exporter, Hosting, all Instrumentation.*)
- [x] Add Serilog packages (AspNetCore, Sinks.Console, Sinks.File, Formatting.Compact)
- [x] Add Health Check packages (AspNetCore.HealthChecks.NpgSql, AspNetCore.HealthChecks.Uris)
- [x] Add YAML configuration package (NetEscapades.Configuration.Yaml)

## Acceptance Criteria
- [x] All Aspire packages version 13.1.0
- [x] OpenTelemetry packages version 1.15.0
- [x] Serilog packages configured
- [x] `dotnet restore` succeeds after changes
- [x] No duplicate package version definitions

## Implementation Details
Packages configured in `Directory.Packages.props`:

| Package Group | Version |
|---------------|---------|
| Aspire.* | 13.1.0 |
| OpenTelemetry.* | 1.15.0 |
| Serilog.* | 9.0.0 / 6.0.0 |
| Health Checks | 9.0.0 |

## Notes
- Aspire.Hosting.Docker is preview version for Docker Compose publishing support
