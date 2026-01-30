# .NET Aspire Orchestration

## Metadata

| Attribute | Value |
|-----------|-------|
| Purpose | Local development orchestration and Docker Compose generation |
| Aspire Version | 13.x |
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
│   ├── EShop.SharedKernel/
│   ├── EShop.Contracts/
│   ├── EShop.Grpc/
│   ├── EShop.Common.Api/
│   ├── EShop.Common.Application/
│   ├── EShop.Common.Infrastructure/
│   └── EShop.ServiceClients/
├── Services/                         # + AddServiceDefaults() in each
│   ├── Gateway/
│   ├── Products/
│   ├── Order/
│   ├── Notification/
│   ├── Analytics/
│   └── DatabaseMigration/
└── EShopDemo.sln
```

---

## 3. AppHost Configuration

The AppHost project is the orchestrator that defines all services and their dependencies.

### 3.1 Key NuGet Packages

- `Aspire.Hosting`
- `Aspire.Hosting.AppHost`
- `Aspire.Hosting.PostgreSQL`
- `Aspire.Hosting.RabbitMQ`

### 3.2 Orchestration Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE                          │
├─────────────────────────────────────────────────────────────┤
│  PostgreSQL (postgres)                                       │
│  ├── productdb                                               │
│  ├── orderdb                                                 │
│  └── notificationdb                                          │
│                                                              │
│  RabbitMQ (messaging)                                        │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   MIGRATION SERVICE                          │
├─────────────────────────────────────────────────────────────┤
│  Database Migration Service                                  │
│    └─ WaitFor: all databases                                 │
│    └─ Runs migrations before services start                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        SERVICES                              │
├─────────────────────────────────────────────────────────────┤
│  Product Service ──────────┐                                 │
│    └─ WaitForCompletion: migration                           │
│    └─ refs: productdb, messaging                             │
│                            │                                 │
│  Order Service ────────────┼──► refs: Product Service (gRPC) │
│    └─ WaitForCompletion: migration                           │
│    └─ refs: orderdb, messaging                               │
│                            │                                 │
│  Notification Service      │                                 │
│    └─ WaitForCompletion: migration                           │
│    └─ refs: notificationdb, messaging                        │
│                                                              │
│  Analytics Service                                           │
│    └─ refs: messaging                                        │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API GATEWAY                             │
├─────────────────────────────────────────────────────────────┤
│  Gateway                                                     │
│    └─ refs: Product Service, Order Service                   │
│    └─ WaitFor: Product Service, Order Service                │
│    └─ external: true (entry point)                           │
└─────────────────────────────────────────────────────────────┘
```

### 3.3 Key Orchestration Concepts

| Concept | Description |
|---------|-------------|
| **WithReference** | Declares dependency and enables service discovery |
| **WaitFor** | Waits for resource to be healthy before starting |
| **WaitForCompletion** | Waits for resource to complete (used for migrations) |
| **WithLifetime(Persistent)** | Container persists between restarts (data preserved) |
| **WithExternalHttpEndpoints** | Marks service as externally accessible |

### 3.4 Database Configuration

- PostgreSQL with PgAdmin for administration
- Three databases: `productdb`, `orderdb`, `notificationdb`
- Persistent container lifetime (data preserved between restarts)

### 3.5 Messaging Configuration

- RabbitMQ with management plugin enabled
- Persistent container lifetime
- Accessed via Aspire service discovery

---

## 4. ServiceDefaults Project

Shared configuration applied to all services for consistent behavior across the solution.

### 4.1 What ServiceDefaults Provides

| Feature | Description |
|---------|-------------|
| **OpenTelemetry** | Logging, metrics, and distributed tracing |
| **Health Checks** | `/health` (readiness) and `/alive` (liveness) endpoints |
| **Service Discovery** | Automatic resolution of service URLs |
| **Resilience** | Standard retry and circuit breaker policies |
| **HTTP Client Defaults** | Pre-configured HttpClient with resilience |

### 4.2 Key Extensions

| Extension | Purpose |
|-----------|---------|
| `AddServiceDefaults()` | Adds all default Aspire configuration |
| `ConfigureOpenTelemetry()` | Sets up logging, metrics, tracing |
| `AddDefaultHealthChecks()` | Adds liveness check |
| `MapDefaultEndpoints()` | Maps `/health` and `/alive` endpoints |

### 4.3 Health Check Endpoints

| Endpoint | Purpose | Includes |
|----------|---------|----------|
| `/health` | Readiness probe | All health checks |
| `/alive` | Liveness probe | Only "live" tagged checks |

---

## 5. Service Integration

Each service must be updated to use ServiceDefaults for consistent behavior.

### 5.1 Service Setup Pattern

Services follow this pattern:
1. Call `AddServiceDefaults()` on the builder
2. Add Aspire-managed resources (database, messaging)
3. Configure service-specific dependencies
4. Call `MapDefaultEndpoints()` on the app

### 5.2 Service Discovery URLs

Services reference each other using Aspire service discovery scheme:
- Format: `https+http://service-name`
- Aspire resolves this automatically based on AppHost configuration
- No hardcoded URLs in appsettings

### 5.3 Connection String Injection

Aspire automatically injects connection strings via environment variables:

| Resource | Environment Variable | Description |
|----------|---------------------|-------------|
| productdb | `ConnectionStrings__productdb` | Product database connection |
| orderdb | `ConnectionStrings__orderdb` | Order database connection |
| notificationdb | `ConnectionStrings__notificationdb` | Notification database connection |
| messaging | `ConnectionStrings__messaging` | RabbitMQ connection |

Services use `builder.AddNpgsqlDbContext<T>("dbname")` which reads the connection string automatically.

---

## 6. Docker Compose Generation

Aspire can generate production-ready Docker Compose files from the AppHost configuration.

### 6.1 Generate Command

```bash
# Generate docker-compose.yaml from AppHost
dotnet run --project src/AppHost -- publish --output-path ./docker
```

### 6.2 Deploy Command

```bash
# Full deployment (build + generate + up)
dotnet run --project src/AppHost -- deploy --output-path ./docker
```

### 6.3 Generated Structure

Generated docker-compose includes:
- All infrastructure containers (PostgreSQL, RabbitMQ)
- All service containers with proper dependencies
- Environment variables for connection strings
- Health checks and startup ordering
- Volume mappings for persistence

---

## 7. Development Workflow

### 7.1 Starting All Services

```bash
# Start everything with Aspire dashboard
dotnet run --project src/AppHost
```

The Aspire dashboard opens automatically and provides:
- Resource overview with health status
- Aggregated logs from all services
- Distributed traces (OpenTelemetry)
- Runtime and HTTP metrics

### 7.2 Aspire Dashboard Features

| Feature | Description |
|---------|-------------|
| **Resources** | View all running services and infrastructure with health status |
| **Console** | Aggregated logs from all services in real-time |
| **Traces** | Distributed traces (OpenTelemetry) across service boundaries |
| **Metrics** | Runtime and HTTP metrics with graphs |
| **Structured Logs** | Searchable structured logs with filtering |

### 7.3 Debugging Individual Services

To debug a single service in IDE while infrastructure runs:
1. Modify AppHost to exclude the service from launch
2. Start AppHost (infrastructure and other services start)
3. Launch the target service from IDE with debugger attached

---

## 8. Compatibility with Existing Patterns

### 8.1 Correlation ID

**Status**: ✅ Fully compatible

Existing `CorrelationContext` works seamlessly with Aspire:
- `CorrelationIdMiddleware` runs in the ASP.NET Core pipeline
- Flows across async boundaries
- OpenTelemetry's `Activity.TraceId` can be used alongside custom correlation ID

### 8.2 gRPC Service Clients

**Status**: ✅ Compatible

`IProductServiceClient` abstraction remains unchanged. Only the URL source changes:
- URLs come from Aspire service discovery instead of appsettings
- Format: `https+http://product-service`

### 8.3 MassTransit/RabbitMQ

**Status**: ✅ Fully compatible

Aspire orchestrates the RabbitMQ container. MassTransit configuration is unchanged:
- Connection string from Aspire (injected via environment variable)
- All consumers, publishers, and patterns work as before

### 8.4 OpenTelemetry

**Status**: ✅ Enhanced

Aspire includes OpenTelemetry by default in ServiceDefaults. The dashboard provides built-in trace visualization, replacing the need for external tools like Jaeger during development.

---

## 9. Production Deployment

### 9.1 Docker Compose (Generated)

```bash
# Generate production-ready docker-compose
dotnet run --project src/AppHost -- publish --output-path ./deploy

# Review and customize if needed
# Deploy
cd deploy && docker-compose up -d
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

## 10. Startup Dependencies

### 10.1 Startup Order

The orchestration ensures proper startup order:

```
1. Infrastructure (PostgreSQL, RabbitMQ)
   └── Start and wait for healthy

2. Migration Service
   └── WaitFor: all databases
   └── Runs migrations, then completes

3. Backend Services (Product, Order, Notification)
   └── WaitForCompletion: migration service
   └── Start after migrations complete

4. Gateway
   └── WaitFor: Product Service, Order Service
   └── Start after backend services are healthy
```

### 10.2 Health Checks

Each service exposes health endpoints:
- `/health` - Readiness (all dependencies healthy)
- `/alive` - Liveness (service is running)

Aspire uses these for startup ordering and dashboard status.

---

## Related Specifications

- [Correlation ID Flow](./correlation-id-flow.md) - Distributed tracing implementation
- [gRPC Communication](./grpc-communication.md) - Inter-service communication
- [Messaging Communication](./messaging-communication.md) - Event-driven architecture
- [Aspire Hybrid Configuration](./aspire-hybrid-configuration.md) - Configuration management
