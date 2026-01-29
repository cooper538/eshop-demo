# Analytics Service - Implementation Specification

## Overview

Analytics service is a Worker service that consumes domain events and logs their processing.
It follows the same patterns as Notification service but with minimal business logic.

---

## 1. Project Structure

```
src/Services/Analytics/
├── Configuration/
│   └── AnalyticsSettings.cs
├── Consumers/
│   └── OrderConfirmedConsumer.cs
├── Data/
│   ├── Configuration/
│   │   └── ProcessedMessageConfiguration.cs
│   ├── Entities/
│   │   └── ProcessedMessage.cs
│   ├── Migrations/
│   │   └── (auto-generated)
│   └── AnalyticsDbContext.cs
├── DependencyInjection.cs
├── Program.cs
├── analytics.settings.yaml
└── EShop.AnalyticsService.csproj
```

---

## 2. Files to Create

### 2.1 Project File

**`EShop.AnalyticsService.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk.Worker">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>EShop.AnalyticsService</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" />
    <PackageReference Include="MassTransit.RabbitMQ" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
    <PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" />
    <PackageReference Include="NetEscapades.Configuration.Yaml" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../ServiceDefaults/EShop.ServiceDefaults.csproj" />
    <ProjectReference Include="../../Common/EShop.Common.Infrastructure/EShop.Common.Infrastructure.csproj" />
    <ProjectReference Include="../../Common/EShop.Contracts/EShop.Contracts.csproj" />
  </ItemGroup>
</Project>
```

### 2.2 Program.cs

```csharp
using EShop.AnalyticsService;
using EShop.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddYamlConfiguration("analytics");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.AddAnalyticsServices();

var host = builder.Build();
host.Run();
```

### 2.3 DependencyInjection.cs

```csharp
using EShop.AnalyticsService.Configuration;
using EShop.AnalyticsService.Consumers;
using EShop.AnalyticsService.Data;
using EShop.Common.Application.Extensions;
using EShop.Common.Infrastructure.Correlation.MassTransit;
using EShop.ServiceDefaults;
using MassTransit;

namespace EShop.AnalyticsService;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddAnalyticsServices(
        this IHostApplicationBuilder builder
    )
    {
        builder
            .Services.AddOptions<AnalyticsSettings>()
            .BindConfiguration(AnalyticsSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.AddNpgsqlDbContext<AnalyticsDbContext>(ResourceNames.Databases.Analytics);

        builder.Services.AddDateTimeProvider();

        builder.Services.AddAnalyticsMessaging(builder.Configuration);

        return builder;
    }

    private static IServiceCollection AddAnalyticsMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderConfirmedConsumer>();

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString(
                        ResourceNames.Messaging
                    );
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        cfg.Host(new Uri(connectionString));
                    }

                    cfg.UseMessageRetry(r =>
                        r.Intervals(
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(15)
                        )
                    );

                    cfg.UseCorrelationIdFilters(context);
                    cfg.ConfigureEndpoints(
                        context,
                        new KebabCaseEndpointNameFormatter("analytics", false)
                    );
                }
            );
        });

        return services;
    }
}
```

### 2.4 Configuration/AnalyticsSettings.cs

```csharp
using System.ComponentModel.DataAnnotations;
using EShop.Common.Application.Configuration;

namespace EShop.AnalyticsService.Configuration;

public class AnalyticsSettings
{
    public const string SectionName = "Analytics";

    [Required]
    public ServiceInfo Service { get; init; } = new();
}
```

### 2.5 analytics.settings.yaml

```yaml
Logging:
  LogLevel:
    Default: "Information"
    Microsoft.AspNetCore: "Warning"

Analytics:
  Service:
    Name: "Analytics"
```

### 2.6 Data/Entities/ProcessedMessage.cs

```csharp
namespace EShop.AnalyticsService.Data.Entities;

// Inbox Pattern - tracks processed messages for idempotency
public class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string ConsumerType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
```

### 2.7 Data/Configuration/ProcessedMessageConfiguration.cs

```csharp
using EShop.AnalyticsService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.AnalyticsService.Data.Configuration;

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");

        builder.HasKey(x => new { x.MessageId, x.ConsumerType });

        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.ConsumerType).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ProcessedAt).IsRequired();

        builder.HasIndex(x => x.ProcessedAt);
    }
}
```

### 2.8 Data/AnalyticsDbContext.cs

```csharp
using EShop.AnalyticsService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.AnalyticsService.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options) { }

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
    }
}
```

### 2.9 Consumers/OrderConfirmedConsumer.cs

```csharp
using EShop.AnalyticsService.Data;
using EShop.AnalyticsService.Data.Entities;
using EShop.Common.Application.Services;
using EShop.Contracts.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.AnalyticsService.Consumers;

public sealed class OrderConfirmedConsumer(
    AnalyticsDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<OrderConfirmedConsumer> logger
) : IConsumer<OrderConfirmedEvent>
{
    private const string ConsumerTypeName = nameof(OrderConfirmedConsumer);

    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();

        // Inbox Pattern - check for duplicate
        var alreadyProcessed = await dbContext.ProcessedMessages.AnyAsync(
            pm => pm.MessageId == messageId && pm.ConsumerType == ConsumerTypeName,
            context.CancellationToken
        );

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Duplicate message {MessageId} detected for {Consumer}. Skipping",
                messageId,
                ConsumerTypeName
            );
            return;
        }

        var message = context.Message;

        // Analytics logging - the main purpose of this consumer
        logger.LogInformation(
            "Analytics: Order {OrderId} confirmed for customer {CustomerEmail}. " +
            "Items: {ItemCount}, Total: {TotalAmount:C}",
            message.OrderId,
            message.CustomerEmail,
            message.Items.Count,
            message.Items.Sum(i => i.UnitPrice * i.Quantity)
        );

        // Record processed message for idempotency
        dbContext.ProcessedMessages.Add(
            new ProcessedMessage
            {
                MessageId = messageId,
                ConsumerType = ConsumerTypeName,
                ProcessedAt = dateTimeProvider.UtcNow,
            }
        );

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
```

---

## 3. Files to Modify

### 3.1 ResourceNames.cs

**Path:** `src/ServiceDefaults/ResourceNames.cs`

Add analytics database:
```csharp
public static class Databases
{
    public const string Product = "productdb";
    public const string Order = "orderdb";
    public const string Notification = "notificationdb";
    public const string Analytics = "analyticsdb";  // ADD THIS
}
```

### 3.2 AppHost Program.cs

**Path:** `src/AppHost/Program.cs`

Add analytics service registration:
```csharp
// After notificationDb definition:
var analyticsDb = postgres.AddDatabase(ResourceNames.Databases.Analytics);

// After notification service definition:
builder
    .AddProject<Projects.EShop_AnalyticsService>("analytics-service")
    .WithReference(analyticsDb)
    .WaitFor(analyticsDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
```

### 3.3 Solution File

**Path:** `EShopDemo.sln`

Add project to solution:
```bash
dotnet sln add src/Services/Analytics/EShop.AnalyticsService.csproj
```

---

## 4. Migration

After creating the project, generate initial migration:

```bash
cd src/Services/Analytics
dotnet ef migrations add InitialCreate --context AnalyticsDbContext
```

---

## 5. Verification Checklist

| Check | Command/Action |
|-------|----------------|
| Build | `dotnet build EShopDemo.sln` |
| Tests | `dotnet test EShopDemo.sln` |
| Format | `dotnet csharpier check .` |
| Run | `dotnet run --project src/AppHost` |

### Manual Verification

1. Open Aspire Dashboard
2. Verify Analytics service is running
3. Open RabbitMQ Management (from Aspire dashboard)
4. Check exchanges - should see `EShop.Contracts:OrderConfirmedEvent`
5. Check queues - should see both:
   - `notification-order-confirmed`
   - `analytics-order-confirmed`
6. Create and confirm an order via Gateway API
7. Check logs - both Notification and Analytics should log the event

---

## 6. Architecture Diagram

```
                                   RabbitMQ
                                      │
  Order Service                       │
       │                              │
       │ Publish                      │
       ▼                              ▼
┌─────────────────────────────────────────────────────────────┐
│  Exchange: EShop.Contracts:OrderConfirmedEvent              │
└───────────────────┬─────────────────┬───────────────────────┘
                    │                 │
           binding  │                 │ binding
                    ▼                 ▼
        ┌───────────────────┐  ┌───────────────────┐
        │ notification-     │  │ analytics-        │
        │ order-confirmed   │  │ order-confirmed   │
        └─────────┬─────────┘  └─────────┬─────────┘
                  │                      │
                  ▼                      ▼
        ┌─────────────────┐    ┌─────────────────┐
        │  Notification   │    │    Analytics    │
        │    Service      │    │     Service     │
        │  (sends email)  │    │  (logs event)   │
        └─────────────────┘    └─────────────────┘
```

---

## 7. Future Enhancements (Out of Scope)

- Add more consumers (OrderRejected, OrderCancelled, StockLow)
- Persist analytics data to dedicated tables
- Add metrics/counters for monitoring
- Integration with external analytics platforms
