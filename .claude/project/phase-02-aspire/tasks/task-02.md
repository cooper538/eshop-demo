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
- [x] Implement `Extensions.cs` with `AddServiceDefaults()` extension method
- [x] Configure OpenTelemetry (logging, metrics, tracing)
- [x] Add default health checks (`/health`, `/alive` endpoints)
- [x] Configure service discovery and HTTP client defaults with resilience
- [x] Add project to solution

## Acceptance Criteria
- [x] Project builds successfully
- [x] `AddServiceDefaults()` method is available as extension on `IHostApplicationBuilder`
- [x] `MapDefaultEndpoints()` method is available as extension on `WebApplication`
- [x] OpenTelemetry instrumentation configured for ASP.NET Core, HTTP, gRPC
- [x] Health check endpoints configured with proper tags

## Notes
- See spec: aspire-orchestration.md, Section 4 (ServiceDefaults Project)
- Namespace: `Microsoft.Extensions.Hosting` (for discoverability)
- Required packages: OpenTelemetry.*, Microsoft.Extensions.ServiceDiscovery, Microsoft.Extensions.Http.Resilience
