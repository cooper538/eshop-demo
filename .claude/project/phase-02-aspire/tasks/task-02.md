# Task 02: ServiceDefaults Project

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Objective
Create EShop.ServiceDefaults project providing shared Aspire configuration for all services.

## Scope
- [ ] Create `src/ServiceDefaults/EShop.ServiceDefaults.csproj` with `IsAspireSharedProject` property
- [ ] Implement `Extensions.cs` with `AddServiceDefaults()` extension method
- [ ] Configure OpenTelemetry (logging, metrics, tracing)
- [ ] Add default health checks (`/health`, `/alive` endpoints)
- [ ] Configure service discovery and HTTP client defaults with resilience
- [ ] Add project to solution

## Acceptance Criteria
- [ ] Project builds successfully
- [ ] `AddServiceDefaults()` method is available as extension on `IHostApplicationBuilder`
- [ ] `MapDefaultEndpoints()` method is available as extension on `WebApplication`
- [ ] OpenTelemetry instrumentation configured for ASP.NET Core, HTTP, gRPC
- [ ] Health check endpoints configured with proper tags

## Notes
- See spec: aspire-orchestration.md, Section 4 (ServiceDefaults Project)
- Namespace: `Microsoft.Extensions.Hosting` (for discoverability)
- Required packages: OpenTelemetry.*, Microsoft.Extensions.ServiceDiscovery, Microsoft.Extensions.Http.Resilience
