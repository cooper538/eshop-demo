# Task 02: ServiceDefaults Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Objective
Create EShop.ServiceDefaults project providing shared Aspire configuration for all services.

## Scope
- [x] Create `src/ServiceDefaults/EShop.ServiceDefaults.csproj` with `IsAspireSharedProject` property
- [x] Implement `ServiceDefaultsExtensions.cs`:
  - [x] `AddServiceDefaults()` - main entry point
  - [x] `ConfigureOpenTelemetry()` - logging, metrics, tracing
  - [x] `AddDefaultHealthChecks()` - returns IHealthChecksBuilder for chaining
  - [x] `MapDefaultEndpoints()` - maps /health and /alive endpoints
  - [x] `AddSerilog()` - Serilog with CorrelationId support
- [x] Implement `ConfigurationExtensions.cs`:
  - [x] `AddYamlConfiguration()` - YAML config file support
- [x] Implement `HealthCheckExtensions.cs`:
  - [x] `AddPostgresHealthCheck()` - PostgreSQL health check by connection string name
  - [x] `AddServiceHealthCheck()` - HTTP health check for downstream services
- [x] Implement `ResourceNames.cs` - centralized resource naming constants

## Acceptance Criteria
- [x] Project builds successfully with `IsAspireSharedProject=true`
- [x] `AddServiceDefaults()` configures OpenTelemetry, health checks, service discovery
- [x] `AddSerilog()` outputs `[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}`
- [x] Health checks support fluent chaining for service-specific checks
- [x] OpenTelemetry instrumentation for ASP.NET Core, HTTP, gRPC, EF Core

## Implementation Details

### Files Created
| File | Purpose |
|------|---------|
| `ServiceDefaultsExtensions.cs` | Core Aspire configuration |
| `ConfigurationExtensions.cs` | YAML configuration support |
| `HealthCheckExtensions.cs` | PostgreSQL and service health checks |
| `ResourceNames.cs` | Centralized resource names |

### OpenTelemetry Configuration
- Activity source pattern: `EShop.*`
- Metrics: AspNetCore, HttpClient, Runtime
- Tracing: AspNetCore, gRPC, HttpClient, EF Core
- OTLP exporter enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` configured

### ResourceNames
```csharp
public static class ResourceNames
{
    public static class Databases
    {
        public const string Product = "productdb";
        public const string Order = "orderdb";
        public const string Notification = "notificationdb";
    }
    public const string Messaging = "messaging";
}
```

## Notes
- Namespace: `Microsoft.Extensions.Hosting` for discoverability
- See spec: aspire-orchestration.md, Section 4 (ServiceDefaults Project)
