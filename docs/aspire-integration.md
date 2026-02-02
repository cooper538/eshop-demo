# Aspire Integration

Local development orchestration with .NET Aspire.

## Running the Application

```bash
dotnet run --project src/AppHost
# Dashboard opens automatically with links to all services
```

Aspire Dashboard provides:
- Service endpoints and logs
- Distributed tracing
- Resource health status

## Adding a New Service

### 1. Add ServiceDefaults Reference

```xml
<!-- YourService.csproj -->
<ProjectReference Include="..\..\ServiceDefaults\EShop.ServiceDefaults.csproj" />
```

### 2. Configure Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                              // OpenTelemetry, health checks, resilience
builder.AddNpgsqlDbContext<YourDbContext>("yourdb");       // If using PostgreSQL
builder.AddRabbitMQClient("messaging");                    // If using RabbitMQ

// Your service configuration...

var app = builder.Build();

app.MapDefaultEndpoints();  // Exposes /health and /alive
app.MapControllers();
app.Run();
```

### 3. Register in AppHost

```csharp
// src/AppHost/Program.cs
builder.AddProject<Projects.YourService>("your-service")
    .WithReference(yourDb)
    .WithReference(rabbitmq)
    .WithReference(otherService);  // If calling another service
```

## Service Communication

Aspire handles service discovery automatically:

```csharp
services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri("https+http://product-service");
})
.AddServiceDiscovery();
```

## Quick Reference

| What | AppHost | Service |
|------|---------|---------|
| PostgreSQL | `postgres.AddDatabase("name")` | `builder.AddNpgsqlDbContext<T>("name")` |
| RabbitMQ | `AddRabbitMQ("name")` | `builder.AddRabbitMQClient("name")` |
| Service ref | `WithReference(service)` | Use service discovery URL |
| Health checks | Automatic | `app.MapDefaultEndpoints()` |
