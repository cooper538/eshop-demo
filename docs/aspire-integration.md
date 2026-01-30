# Service Aspire Integration Guide

This guide documents how to integrate services with .NET Aspire orchestration.

## Overview

Each service in the EShop solution uses:
- **ServiceDefaults** - Shared configuration (OpenTelemetry, health checks, resilience)
- **AppHost** - Orchestration and resource management
- **Aspire components** - Database and messaging integrations

## Service Program.cs Pattern

```csharp
var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// ASPIRE INTEGRATION
// ═══════════════════════════════════════════════════════════════

// Add shared defaults (OpenTelemetry, health checks, service discovery, resilience)
builder.AddServiceDefaults();

// Aspire-managed PostgreSQL (connection string injected automatically)
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");

// Aspire-managed RabbitMQ (connection string injected automatically)
builder.AddRabbitMQClient("messaging");

// ═══════════════════════════════════════════════════════════════
// SERVICE CONFIGURATION
// ═══════════════════════════════════════════════════════════════

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
// ... additional service configuration

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// ASPIRE ENDPOINTS
// ═══════════════════════════════════════════════════════════════

// Map health check endpoints (/health, /alive)
app.MapDefaultEndpoints();

// ═══════════════════════════════════════════════════════════════
// MIDDLEWARE & ENDPOINTS
// ═══════════════════════════════════════════════════════════════

app.UseExceptionHandler();
// ... middleware pipeline
app.MapControllers();

app.Run();
```

## AppHost Service Registration

Register services in `src/AppHost/Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

// Services
var productService = builder.AddProject<Projects.EShop_ProductService>("product-service")
    .WithReference(productDb)
    .WithReference(rabbitmq)
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithHttpsEndpoint(port: 5051, name: "grpc");

var orderService = builder.AddProject<Projects.EShop_OrderService>("order-service")
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WithReference(productService);  // gRPC dependency

builder.Build().Run();
```

## Service Discovery

Services reference each other using Aspire service discovery URLs:

```csharp
// Service discovery URL format
// "https+http://service-name" - Aspire resolves this automatically

services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri("https+http://product-service");
})
.AddServiceDiscovery();
```

## Database Integration

Use Aspire's `AddNpgsqlDbContext<T>()` for automatic connection string injection:

```csharp
// In service Program.cs
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");

// Connection string is automatically read from:
// - Environment variable: ConnectionStrings__productdb
// - Injected by Aspire at runtime
```

## Messaging Integration

Use Aspire's `AddRabbitMQClient()` for automatic connection:

```csharp
// In service Program.cs
builder.AddRabbitMQClient("messaging");

// Connection string is automatically read from:
// - Environment variable: ConnectionStrings__messaging
// - Injected by Aspire at runtime
```

## Health Checks

`MapDefaultEndpoints()` exposes two endpoints:

| Endpoint | Purpose | Checks |
|----------|---------|--------|
| `/health` | Readiness probe | All registered health checks |
| `/alive` | Liveness probe | Only checks tagged with "live" |

## Required Project References

### Service Project (.csproj)

```xml
<ItemGroup>
  <ProjectReference Include="..\..\ServiceDefaults\EShop.ServiceDefaults.csproj" />
</ItemGroup>

<ItemGroup>
  <!-- For PostgreSQL -->
  <PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" />

  <!-- For RabbitMQ -->
  <PackageReference Include="Aspire.RabbitMQ.Client" />
</ItemGroup>
```

### AppHost Project (.csproj)

```xml
<ItemGroup>
  <ProjectReference Include="..\Services\Product\EShop.ProductService.csproj" />
</ItemGroup>
```

## Running with Aspire

```bash
# Start all services with Aspire dashboard
dotnet run --project src/AppHost

# Access points:
# - Aspire Dashboard: https://localhost:17225
# - Services: Configured ports in AppHost
```
