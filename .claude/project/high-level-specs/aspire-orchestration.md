# .NET Aspire Orchestration

## Metadata

| Attribute | Value |
|-----------|-------|
| Purpose | Local development orchestration and Docker Compose generation |
| Aspire Version | 9.0.0 |
| Replaces | Manual docker-compose.yml configuration |

---

## 1. Overview

.NET Aspire provides cloud-ready development experience for distributed .NET applications:
- **Local orchestration**: One-click start for all services and infrastructure
- **Service discovery**: Automatic URL injection between services
- **Observability**: Built-in dashboard with logs, traces, and metrics
- **Docker Compose generation**: `aspire publish` for production deployment

### 1.1 Why Aspire for This Project

| Benefit | Description |
|---------|-------------|
| Developer Experience | Single command starts all services, databases, and message brokers |
| Service Discovery | No hardcoded URLs - services find each other automatically |
| Observability | Built-in dashboard with OpenTelemetry integration |
| Production Artifacts | Generates docker-compose.yaml from C# code |
| Consistency | Same orchestration model for dev and prod |

---

## 2. Project Structure

```
src/
├── AppHost/                          # Aspire orchestrator
│   ├── EShop.AppHost.csproj
│   ├── Program.cs
│   └── appsettings.json
├── ServiceDefaults/                  # Shared Aspire configuration
│   ├── EShop.ServiceDefaults.csproj
│   └── Extensions.cs
├── Common/                           # Unchanged
│   ├── EShop.Common/
│   ├── EShop.Contracts/
│   ├── EShop.Grpc/
│   └── EShop.ServiceClients/
├── Services/                         # + AddServiceDefaults() in each
│   ├── Gateway/
│   ├── Product/
│   ├── Order/
│   └── Notification/
└── EShopDemo.sln
```

---

## 3. AppHost Configuration

The AppHost project is the orchestrator that defines all services and their dependencies.

### 3.1 Project File

**File**: `src/AppHost/EShop.AppHost.csproj`

```xml
<Project Sdk="Aspire.AppHost.Sdk/9.0.0">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.PostgreSQL" />
    <PackageReference Include="Aspire.Hosting.RabbitMQ" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Services\Gateway\EShop.Gateway.csproj" />
    <ProjectReference Include="..\Services\Product\EShop.ProductService.csproj" />
    <ProjectReference Include="..\Services\Order\EShop.OrderService.csproj" />
    <ProjectReference Include="..\Services\Notification\EShop.NotificationService.csproj" />
  </ItemGroup>
</Project>
```

### 3.2 Orchestration Code

**File**: `src/AppHost/Program.cs`

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// INFRASTRUCTURE
// ═══════════════════════════════════════════════════════════════

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");
var orderDb = postgres.AddDatabase("orderdb");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

// ═══════════════════════════════════════════════════════════════
// SERVICES
// ═══════════════════════════════════════════════════════════════

var productService = builder.AddProject<Projects.EShop_ProductService>("product-service")
    .WithReference(productDb)
    .WithReference(rabbitmq)
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithHttpsEndpoint(port: 5051, name: "grpc");

var orderService = builder.AddProject<Projects.EShop_OrderService>("order-service")
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WithReference(productService)  // gRPC dependency - enables service discovery
    .WithHttpsEndpoint(port: 5002, name: "https");

var notificationService = builder.AddProject<Projects.EShop_NotificationService>("notification-service")
    .WithReference(rabbitmq);

// ═══════════════════════════════════════════════════════════════
// API GATEWAY (Entry Point)
// ═══════════════════════════════════════════════════════════════

var gateway = builder.AddProject<Projects.EShop_Gateway>("gateway")
    .WithReference(productService)
    .WithReference(orderService)
    .WithHttpsEndpoint(port: 5000, name: "https")
    .WithExternalHttpEndpoints();  // Marks as external entry point

builder.Build().Run();
```

### 3.3 Resource Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE                          │
├─────────────────────────────────────────────────────────────┤
│  PostgreSQL (postgres)                                       │
│  ├── productdb                                               │
│  └── orderdb                                                 │
│                                                              │
│  RabbitMQ (messaging)                                        │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        SERVICES                              │
├─────────────────────────────────────────────────────────────┤
│  Product Service ──────────┐                                 │
│    └─ refs: productdb,     │                                 │
│            messaging       │                                 │
│                            │                                 │
│  Order Service ────────────┼──► refs: Product Service (gRPC) │
│    └─ refs: orderdb,       │                                 │
│            messaging       │                                 │
│                            │                                 │
│  Notification Service      │                                 │
│    └─ refs: messaging      │                                 │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API GATEWAY                             │
├─────────────────────────────────────────────────────────────┤
│  Gateway                                                     │
│    └─ refs: Product Service, Order Service                   │
│    └─ external: true (entry point)                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. ServiceDefaults Project

Shared configuration applied to all services for consistent behavior across the solution.

### 4.1 Project File

**File**: `src/ServiceDefaults/EShop.ServiceDefaults.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAspireSharedProject>true</IsAspireSharedProject>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />

    <!-- Resilience -->
    <PackageReference Include="Microsoft.Extensions.Http.Resilience" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" />

    <!-- OpenTelemetry -->
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" />
    <PackageReference Include="OpenTelemetry.Instrumentation.GrpcNetClient" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" />
  </ItemGroup>
</Project>
```

### 4.2 Extensions Implementation

**File**: `src/ServiceDefaults/Extensions.cs`

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    /// <summary>
    /// Adds common Aspire service defaults including OpenTelemetry, health checks,
    /// service discovery, and resilience policies.
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry for logging, metrics, and tracing.
    /// </summary>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;  // Important for CorrelationId propagation
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Adds default health checks for liveness and readiness probes.
    /// </summary>
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps the default health check endpoints (/health, /alive).
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Readiness check - all health checks
        app.MapHealthChecks("/health");

        // Liveness check - only "live" tagged checks
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
```

### 4.3 What ServiceDefaults Provides

| Feature | Description |
|---------|-------------|
| **OpenTelemetry** | Logging, metrics, and distributed tracing |
| **Health Checks** | `/health` (readiness) and `/alive` (liveness) endpoints |
| **Service Discovery** | Automatic resolution of service URLs |
| **Resilience** | Standard retry and circuit breaker policies |
| **HTTP Client Defaults** | Pre-configured HttpClient with resilience |

---

## 5. Service Integration

Each service must be updated to use ServiceDefaults for consistent behavior.

### 5.1 Service Program.cs Pattern

```csharp
var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// ASPIRE INTEGRATION
// ═══════════════════════════════════════════════════════════════

builder.AddServiceDefaults();

// Aspire-managed PostgreSQL (connection string injected automatically)
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");

// Aspire-managed RabbitMQ (connection string injected automatically)
builder.AddRabbitMQClient("messaging");

// ═══════════════════════════════════════════════════════════════
// EXISTING CONFIGURATION (unchanged)
// ═══════════════════════════════════════════════════════════════

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddCorrelationId();  // Custom correlation ID still works
// ... rest of service configuration

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// ASPIRE ENDPOINTS
// ═══════════════════════════════════════════════════════════════

app.MapDefaultEndpoints();

// ═══════════════════════════════════════════════════════════════
// EXISTING MIDDLEWARE (unchanged)
// ═══════════════════════════════════════════════════════════════

app.UseCorrelationId();
app.UseExceptionHandler();
// ... rest of middleware pipeline

app.Run();
```

### 5.2 Service Discovery URLs

Services reference each other using Aspire service discovery scheme instead of hardcoded URLs:

```csharp
// ═══════════════════════════════════════════════════════════════
// BEFORE (hardcoded in appsettings.json)
// ═══════════════════════════════════════════════════════════════

// appsettings.json
{
  "ServiceClients": {
    "ProductService": {
      "Url": "https://localhost:5051"
    }
  }
}

// Registration
services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri(config["ServiceClients:ProductService:Url"]);
});

// ═══════════════════════════════════════════════════════════════
// AFTER (Aspire service discovery)
// ═══════════════════════════════════════════════════════════════

// Registration
services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    // "https+http://product-service" - Aspire resolves this automatically
    options.Address = new Uri("https+http://product-service");
})
.AddServiceDiscovery();  // Enable service discovery for this client
```

### 5.3 Connection String Injection

Aspire automatically injects connection strings via environment variables:

| Resource | Environment Variable | Example Value |
|----------|---------------------|---------------|
| productdb | `ConnectionStrings__productdb` | `Host=localhost;Database=productdb;...` |
| orderdb | `ConnectionStrings__orderdb` | `Host=localhost;Database=orderdb;...` |
| messaging | `ConnectionStrings__messaging` | `amqp://guest:guest@localhost:5672` |

Services use these via `builder.AddNpgsqlDbContext<T>("productdb")` which reads the connection string automatically.

---

## 6. Docker Compose Generation

Aspire can generate production-ready Docker Compose files from the AppHost configuration.

### 6.1 Generate Command

```bash
# Generate docker-compose.yaml from AppHost
dotnet run --project src/AppHost -- publish --output-path ./docker

# Output:
# docker/
# ├── docker-compose.yaml      # Generated compose file
# ├── .env                     # Environment variables
# └── aspirate.json            # Aspire metadata
```

### 6.2 Deploy Command

```bash
# Full deployment (build + generate + up)
dotnet run --project src/AppHost -- deploy --output-path ./docker
```

### 6.3 Generated Docker Compose Structure

```yaml
# docker/docker-compose.yaml (generated)
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "15672:15672"
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_port_connectivity"]

  product-service:
    build:
      context: ../src/Services/Product
    environment:
      - ConnectionStrings__productdb=${PRODUCTDB_CONNECTION}
      - ConnectionStrings__messaging=${MESSAGING_CONNECTION}
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  # ... other services

volumes:
  postgres-data:
```

---

## 7. Development Workflow

### 7.1 Starting All Services

```bash
# Start everything with Aspire dashboard
dotnet run --project src/AppHost

# Access points:
# - Aspire Dashboard: https://localhost:17225
# - API Gateway: https://localhost:5000
# - Product Service: https://localhost:5001 (HTTP), https://localhost:5051 (gRPC)
# - Order Service: https://localhost:5002
# - RabbitMQ Management: http://localhost:15672
# - pgAdmin: http://localhost:5050
```

### 7.2 Aspire Dashboard Features

| Feature | Description |
|---------|-------------|
| **Resources** | View all running services and infrastructure with health status |
| **Console** | Aggregated logs from all services in real-time |
| **Traces** | Distributed traces (OpenTelemetry) across service boundaries |
| **Metrics** | Runtime and HTTP metrics with graphs |
| **Structured Logs** | Searchable structured logs with filtering |

### 7.3 Debugging Individual Services

```bash
# Start only infrastructure (for debugging a single service in IDE)
dotnet run --project src/AppHost -- --no-launch product-service

# Then launch Product Service from your IDE with debugger attached
```

---

## 8. Compatibility with Existing Patterns

### 8.1 Correlation ID

**Status**: ✅ Fully compatible

Existing `AsyncLocal<T>` based `CorrelationContext` works seamlessly with Aspire. The custom correlation ID implementation continues to work because:

- `CorrelationIdMiddleware` runs in the ASP.NET Core pipeline
- `AsyncLocal<T>` propagates across async boundaries
- OpenTelemetry's `Activity.TraceId` can be used alongside or mapped to custom correlation ID

```csharp
// Both can coexist
var correlationId = CorrelationContext.Current?.CorrelationId;  // Custom
var traceId = Activity.Current?.TraceId.ToString();             // OpenTelemetry
```

### 8.2 gRPC Service Clients

**Status**: ✅ Compatible with minor changes

`IProductServiceClient` abstraction remains unchanged. Only the URL source changes:

```csharp
// IProductServiceClient interface - NO CHANGES
public interface IProductServiceClient
{
    Task<StockReservationResult> ReserveStockAsync(ReserveStockRequest request, CancellationToken ct);
    Task<StockReleaseResult> ReleaseStockAsync(ReleaseStockRequest request, CancellationToken ct);
}

// Registration - URL from Aspire instead of appsettings
services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri("https+http://product-service");
})
.AddServiceDiscovery();
```

### 8.3 MassTransit/RabbitMQ

**Status**: ✅ Fully compatible

Aspire orchestrates the RabbitMQ container. MassTransit configuration is unchanged:

```csharp
// MassTransit configuration - unchanged
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Connection string from Aspire (injected via environment variable)
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.UseCorrelationIdFilters();
        cfg.ConfigureEndpoints(context);
    });
});
```

### 8.4 OpenTelemetry

**Status**: ✅ Enhanced

Aspire includes OpenTelemetry by default in ServiceDefaults. The dashboard provides built-in trace visualization, replacing the need for external tools like Jaeger or Seq during development.

---

## 9. Production Deployment

### 9.1 Docker Compose (Generated)

```bash
# Generate production-ready docker-compose
dotnet run --project src/AppHost -- publish --output-path ./deploy

# Review and customize if needed
vim ./deploy/docker-compose.yaml

# Deploy
cd deploy
docker-compose up -d
```

### 9.2 Azure Container Apps

```bash
# Initialize Azure Developer CLI
azd init

# Deploy to Azure Container Apps
azd up
```

### 9.3 Kubernetes (via Aspir8)

```bash
# Install Aspir8 tool
dotnet tool install -g aspirate

# Generate Kubernetes manifests
aspirate generate --output-format kustomize

# Apply to cluster
kubectl apply -k ./output
```

---

## 10. Package Requirements

### 10.1 Directory.Packages.props Additions

```xml
<PropertyGroup>
  <AspireVersion>9.0.0</AspireVersion>
</PropertyGroup>

<ItemGroup>
  <!-- Aspire Hosting (AppHost only) -->
  <PackageVersion Include="Aspire.Hosting" Version="$(AspireVersion)" />
  <PackageVersion Include="Aspire.Hosting.AppHost" Version="$(AspireVersion)" />
  <PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="$(AspireVersion)" />
  <PackageVersion Include="Aspire.Hosting.RabbitMQ" Version="$(AspireVersion)" />

  <!-- Aspire Components (for services) -->
  <PackageVersion Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="$(AspireVersion)" />
  <PackageVersion Include="Aspire.RabbitMQ.Client" Version="$(AspireVersion)" />

  <!-- Service Discovery -->
  <PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="$(AspireVersion)" />

  <!-- Resilience -->
  <PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="9.0.0" />

  <!-- OpenTelemetry -->
  <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
  <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.GrpcNetClient" Version="1.9.0-beta.1" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />
</ItemGroup>
```

---

## 11. Validation Checklist

### Development Environment

- [ ] `dotnet run --project src/AppHost` starts all services
- [ ] Aspire dashboard accessible at https://localhost:17225
- [ ] All services show "Running" status in dashboard
- [ ] PostgreSQL and RabbitMQ containers are healthy
- [ ] Service-to-service communication works (Order → Product gRPC)
- [ ] Integration events flow through RabbitMQ
- [ ] Distributed traces visible in Aspire dashboard
- [ ] Correlation IDs propagate across services

### Production Artifacts

- [ ] `aspire publish` generates valid docker-compose.yaml
- [ ] Generated docker-compose starts successfully with `docker-compose up`
- [ ] Services communicate correctly in containerized environment
- [ ] Health checks pass for all services

---

## Related Specifications

- [Correlation ID Flow](./correlation-id-flow.md) - Distributed tracing implementation
- [gRPC Communication](./grpc-communication.md) - Inter-service communication
- [Messaging Communication](./messaging-communication.md) - Event-driven architecture
- 
