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
- [x] Document Program.cs pattern for services
- [x] Document service registration in AppHost pattern
- [x] Document usage patterns:
  - [x] `AddServiceDefaults()` usage
  - [x] `AddSerilog()` with service name
  - [x] `AddYamlConfiguration()` for YAML config files
  - [x] Health check chaining pattern
  - [x] `MapDefaultEndpoints()` usage
  - [x] Service discovery URL format

## Acceptance Criteria
- [x] Pattern is documented and followed by all services
- [x] Consistent service implementation across all phases
- [x] Health checks properly configured for each service

## Implementation Details

### Service Program.cs Pattern
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSerilog();
builder.AddYamlConfiguration("service-name");

// Add service-specific configuration
builder.AddDefaultHealthChecks()
    .AddPostgresHealthCheck(ResourceNames.Databases.ServiceDb);

var app = builder.Build();
app.MapDefaultEndpoints();
// Map service endpoints
app.Run();
```

### AppHost Service Registration Pattern
```csharp
var service = builder
    .AddProject<Projects.Service_API>("service-name")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithReference(database)
    .WithReference(rabbitmq)
    .WaitForCompletion(migrationService)
    .WaitFor(rabbitmq);
```

### Health Check Chaining
```csharp
builder.AddDefaultHealthChecks()
    .AddPostgresHealthCheck(ResourceNames.Databases.Product)
    .AddServiceHealthCheck("downstream-service");
```

### Service Discovery
- Internal: `http://service-name` (resolved by Aspire)
- Gateway routes to internal services via YARP with service discovery

## Notes
- See spec: aspire-orchestration.md, Section 5 (Service Integration)
- See spec: aspire-hybrid-configuration.md, Section 5 (Options Pattern)
- Pattern implemented consistently across Product, Order, Notification, Analytics, Gateway services
