# Shared Projects Architecture

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Shared project structure and data access patterns |
| Applies To | All services |
| Data Access | Direct DbContext via interface (NO Repository pattern) |

---

## 1. Overview

This document describes the shared projects structure and the data access pattern used across all microservices.

### 1.1 Key Principles

- **SharedKernel has ZERO dependencies** - pure DDD building blocks
- **NO Repository pattern** - direct DbContext access via interface
- **Contracts are minimal** - only events and DTOs, no logic
- **gRPC contracts are shared** - both client and server stubs generated
- **ServiceClients abstract protocol** - gRPC/HTTP switching via configuration

---

## 2. Project Dependency Graph

```
                        ┌─────────────────────────┐
                        │   EShop.SharedKernel    │  ← ZERO dependencies
                        │   (Entity, Aggregate,   │
                        │    ValueObject, Guards) │
                        └───────────┬─────────────┘
                                    │
               ┌────────────────────┼────────────────────┐
               │                    │                    │
               ▼                    ▼                    ▼
    ┌──────────────────┐  ┌─────────────────┐  ┌────────────────────┐
    │  EShop.Contracts │  │   EShop.Grpc    │  │    EShop.Common    │
    │  (Events, DTOs)  │  │   (Protos)      │  │  (Infrastructure)  │
    └────────┬─────────┘  └────────┬────────┘  └─────────┬──────────┘
             │                     │                     │
             └──────────────┬──────┴─────────────────────┘
                            │
                            ▼
                 ┌─────────────────────────┐
                 │  EShop.ServiceClients   │
                 │  (Dual-protocol)        │
                 └─────────────────────────┘
                            │
                            ▼
                 ┌─────────────────────────┐
                 │    Service Projects     │
                 │  (Order, Product, ...)  │
                 └─────────────────────────┘
```

---

## 3. Shared Projects Detail

### 3.1 EShop.SharedKernel

**Purpose:** Pure DDD building blocks with ZERO external dependencies.

**NuGet Dependencies:** NONE (only .NET SDK)

**Location:** `src/Common/EShop.SharedKernel/`

```
EShop.SharedKernel/
├── Domain/
│   ├── Entity.cs                 # Base entity with Id and domain events
│   ├── AggregateRoot.cs          # Aggregate root marker + event collection
│   ├── ValueObject.cs            # Value object with equality
│   └── IAggregateRoot.cs         # Marker interface
├── Events/
│   ├── IDomainEvent.cs           # Domain event interface
│   └── DomainEventBase.cs        # Base record with timestamp
└── Guards/
    └── Guard.cs                  # Guard clauses (Against.Null, etc.)
```

#### 3.1.1 Entity Base Class

```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

#### 3.1.2 AggregateRoot

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public uint Version { get; protected set; }
}

public interface IAggregateRoot { }
```

#### 3.1.3 ValueObject

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
```

#### 3.1.4 Guard Clauses

```csharp
public static class Guard
{
    public static class Against
    {
        public static T Null<T>(T? value, string parameterName) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(parameterName);
            return value;
        }

        public static string NullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", parameterName);
            return value;
        }

        public static int NegativeOrZero(int value, string parameterName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
            return value;
        }

        public static decimal Negative(decimal value, string parameterName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
            return value;
        }
    }
}
```

---

### 3.2 EShop.Contracts

**Purpose:** Integration events and shared DTOs for cross-service communication.

**NuGet Dependencies:** `EShop.SharedKernel` (optional, for IDomainEvent if needed)

**Location:** `src/Common/EShop.Contracts/`

```
EShop.Contracts/
├── Events/
│   ├── IntegrationEvent.cs       # Base record for all events
│   ├── Order/
│   │   ├── OrderConfirmedEvent.cs
│   │   ├── OrderRejectedEvent.cs
│   │   └── OrderCancelledEvent.cs
│   └── Product/
│       ├── StockLowEvent.cs
│       ├── ProductCreatedEvent.cs
│       └── ProductUpdatedEvent.cs
├── Dtos/
│   ├── Order/
│   │   ├── OrderItemDto.cs
│   │   └── OrderSummaryDto.cs
│   └── Product/
│       ├── ProductDto.cs
│       └── StockInfoDto.cs
└── Constants/
    └── EventNames.cs             # Constants for event routing
```

#### 3.2.1 Base Integration Event

```csharp
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    // Note: CorrelationId is propagated via MassTransit message headers (implicit)
    // See: correlation-id-flow.md for details
}
```

#### 3.2.2 Example Events

```csharp
// Order Events
public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string CustomerEmail) : IntegrationEvent;

public sealed record OrderRejectedEvent(
    Guid OrderId,
    Guid CustomerId,
    string Reason) : IntegrationEvent;

public sealed record OrderCancelledEvent(
    Guid OrderId,
    string Reason) : IntegrationEvent;

// Product Events
public sealed record StockLowEvent(
    Guid ProductId,
    string ProductName,
    int CurrentQuantity,
    int Threshold) : IntegrationEvent;
```

---

### 3.3 EShop.Grpc

**Purpose:** Proto definitions and generated C# code for gRPC communication.

**NuGet Dependencies:** `Grpc.Tools`, `Google.Protobuf`, `Grpc.Net.Client`

**Location:** `src/Common/EShop.Grpc/`

```
EShop.Grpc/
├── Protos/
│   └── product.proto
└── EShop.Grpc.csproj
```

#### 3.3.1 Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Protobuf Include="Protos\**\*.proto" GrpcServices="Both" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Grpc.Tools" PrivateAssets="All" />
    <PackageReference Include="Google.Protobuf" />
    <PackageReference Include="Grpc.Net.Client" />
  </ItemGroup>
</Project>
```

**Key:** `GrpcServices="Both"` generates both client and server code.

#### 3.3.2 Product Proto Definition

```protobuf
syntax = "proto3";

option csharp_namespace = "EShop.Grpc.Product";
package product;

service ProductService {
  rpc ReserveStock (ReserveStockRequest) returns (ReserveStockResponse);
  rpc ReleaseStock (ReleaseStockRequest) returns (ReleaseStockResponse);
}

message ReserveStockRequest {
  string order_id = 1;
  reserved 2;  // Was: correlation_id - now propagated via gRPC metadata
  repeated OrderItem items = 3;
}

message OrderItem {
  string product_id = 1;
  int32 quantity = 2;
}

message ReserveStockResponse {
  bool success = 1;
  optional string failure_reason = 2;
  repeated string failed_product_ids = 3;
}

message ReleaseStockRequest {
  string order_id = 1;
  reserved 2;  // Was: correlation_id - now propagated via gRPC metadata
}

message ReleaseStockResponse {
  bool success = 1;
  optional string failure_reason = 2;
}
```

#### 3.3.3 Why Separate Project?

- **Server** (Product Service) needs `ProductService.ProductServiceBase` to implement
- **Client** (Order Service via ServiceClients) needs `ProductService.ProductServiceClient` to call
- **Both** need the same message classes
- Keeps proto in one place, avoids version mismatch

---

### 3.4 EShop.Common

**Purpose:** Shared infrastructure - middleware, behaviors, exceptions.

**NuGet Dependencies:**
- `EShop.SharedKernel`
- `MediatR`
- `FluentValidation`
- `Microsoft.AspNetCore.*`
- `Polly`

**Location:** `src/Common/EShop.Common/`

```
EShop.Common/
├── Behaviors/                    # MediatR pipeline
│   ├── ValidationBehavior.cs
│   ├── LoggingBehavior.cs
│   └── TransactionBehavior.cs
├── Exceptions/
│   ├── NotFoundException.cs
│   ├── ValidationException.cs
│   ├── ConflictException.cs
│   └── DomainException.cs
├── Middleware/
│   ├── CorrelationIdMiddleware.cs
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Correlation/
│   ├── ICorrelationContext.cs
│   ├── CorrelationContext.cs
│   └── CorrelationIdConstants.cs
├── Grpc/
│   ├── CorrelationIdClientInterceptor.cs
│   └── CorrelationIdServerInterceptor.cs
├── Messaging/
│   ├── Outbox/
│   │   ├── OutboxMessage.cs
│   │   ├── IOutboxRepository.cs
│   │   └── OutboxProcessor.cs
│   └── Inbox/
│       ├── ProcessedMessage.cs
│       └── IdempotentConsumer.cs
└── Extensions/
    ├── ServiceCollectionExtensions.cs
    └── ConfigurationExtensions.cs
```

---

### 3.5 EShop.ServiceClients

**Purpose:** Dual-protocol abstraction for inter-service communication.

**NuGet Dependencies:**
- `EShop.Grpc`
- `EShop.Common`
- `Grpc.Net.Client`
- `Polly`

**Location:** `src/Common/EShop.ServiceClients/`

```
EShop.ServiceClients/
├── Abstractions/
│   └── IProductServiceClient.cs
├── Models/
│   ├── ReserveStockRequest.cs
│   ├── StockReservationResult.cs
│   ├── ReleaseStockRequest.cs
│   └── StockReleaseResult.cs
├── Grpc/
│   └── GrpcProductServiceClient.cs
├── Http/
│   └── HttpProductServiceClient.cs
├── Configuration/
│   └── ServiceClientOptions.cs
├── Exceptions/
│   └── ServiceClientException.cs
├── Resilience/
│   └── ResiliencePolicies.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

See [Dual-Protocol Communication](./dual-protocol-communication.md) for implementation details.

---

## 4. Data Access Pattern

### 4.1 NO Repository Pattern

We use **direct DbContext access via interface** instead of Repository pattern.

**Why:**
- Simpler - no extra abstraction layer
- Full EF Core power - LINQ, Include, projections, raw SQL
- Interface in Application layer - still testable
- Less code to maintain

### 4.2 Interface in Application Layer

Each service defines its DbContext interface in the **Application layer**:

```csharp
// Product.Application/Data/IProductDbContext.cs
public interface IProductDbContext
{
    DbSet<Product> Products { get; }
    DbSet<StockReservation> StockReservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 4.3 Implementation in Infrastructure Layer

```csharp
// Product.Infrastructure/Data/ProductDbContext.cs
public class ProductDbContext : DbContext, IProductDbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}
```

### 4.4 Registration in DI

```csharp
// Product.Infrastructure/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddDbContext<ProductDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("ProductDb")));

    // Register interface pointing to concrete DbContext
    services.AddScoped<IProductDbContext>(sp =>
        sp.GetRequiredService<ProductDbContext>());

    return services;
}
```

### 4.5 Usage in Handlers

```csharp
public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductDbContext _db;

    public GetProductByIdQueryHandler(IProductDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken ct)
    {
        return await _db.Products
            .Where(p => p.Id == request.ProductId)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Price,
                p.StockQuantity))
            .FirstOrDefaultAsync(ct);
    }
}
```

### 4.6 Testing with InMemory DbContext

```csharp
public class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ProductDbContext(options);
        var product = Product.Create("Test Product", 99.99m, 100);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(
            new GetProductByIdQuery(product.Id),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
    }
}
```

---

## 5. Summary Table

| Project | Purpose | Dependencies | Key Contents |
|---------|---------|--------------|--------------|
| **SharedKernel** | DDD building blocks | NONE | Entity, AggregateRoot, ValueObject, Guards |
| **Contracts** | Cross-service contracts | SharedKernel (optional) | IntegrationEvents, Shared DTOs |
| **Grpc** | gRPC contracts | Grpc.Tools, Protobuf | Proto files, generated client/server |
| **Common** | Shared infrastructure | SharedKernel, MediatR, Polly | Behaviors, Middleware, Outbox/Inbox |
| **ServiceClients** | Protocol abstraction | Grpc, Common | IProductServiceClient, gRPC/HTTP impls |
| **ServiceDefaults** | Aspire config | Aspire | OpenTelemetry, Health checks |

---

## 6. Future Extension: NuGet Packages

### 6.1 When to Consider

Current approach uses **project references** which is ideal for:
- Single team/repo development
- Rapid iteration and debugging
- Demo/learning projects

Consider migrating to **internal NuGet packages** when:
- Multiple teams work on different services
- Services live in separate repositories
- Need strict versioning and breaking change management
- Organization scales beyond single solution

### 6.2 Package Candidates

| Project | Package Name | Versioning | Notes |
|---------|--------------|------------|-------|
| **SharedKernel** | `EShop.SharedKernel` | SemVer | Rarely changes, stable API |
| **Contracts** | `EShop.Contracts` | SemVer | **Critical** - breaking changes affect all consumers |
| **Grpc** | `EShop.Grpc` | SemVer | Tied to proto changes |
| **Common** | `EShop.Common` | SemVer | Infrastructure utilities |
| **ServiceClients** | `EShop.ServiceClients` | SemVer | Protocol abstractions |

### 6.3 Recommended Package Structure

```
packages/
├── EShop.SharedKernel/
│   ├── src/
│   ├── tests/
│   └── EShop.SharedKernel.csproj    # <IsPackable>true</IsPackable>
├── EShop.Contracts/
│   ├── src/
│   └── EShop.Contracts.csproj
└── ...

# Each package has its own:
# - Version (in .csproj or Directory.Build.props)
# - CHANGELOG.md
# - CI/CD pipeline for publishing
```

### 6.4 NuGet Project Configuration

```xml
<!-- Example: EShop.SharedKernel.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>true</IsPackable>
    <PackageId>EShop.SharedKernel</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Team</Authors>
    <Description>DDD building blocks for EShop microservices</Description>
    <PackageTags>ddd;domain;microservices</PackageTags>
    <RepositoryUrl>https://github.com/your-org/eshop-packages</RepositoryUrl>
  </PropertyGroup>
</Project>
```

### 6.5 Publishing Strategy

```yaml
# .github/workflows/publish-sharedkernel.yml
name: Publish SharedKernel

on:
  push:
    tags:
      - 'sharedkernel-v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet pack packages/EShop.SharedKernel -c Release
      - run: dotnet nuget push **/*.nupkg --source https://nuget.pkg.github.com/your-org/index.json
```

### 6.6 Versioning Guidelines for Packages

| Change Type | Version Bump | Example |
|-------------|--------------|---------|
| Bug fix, no API change | PATCH | 1.0.0 → 1.0.1 |
| New feature, backward compatible | MINOR | 1.0.1 → 1.1.0 |
| Breaking change | MAJOR | 1.1.0 → 2.0.0 |

**Breaking changes in Contracts require coordination:**
1. Publish new version with `[Obsolete]` attributes
2. Update all consumers
3. Remove obsolete code in next major version

### 6.7 Migration Path

```
Phase 1: Current (Project References)
  └── All projects in single solution
  └── Direct references: <ProjectReference Include="..." />

Phase 2: Hybrid (Gradual Migration)
  └── SharedKernel + Contracts as NuGet
  └── Services still use project references for Common/ServiceClients

Phase 3: Full NuGet (Multi-Repo)
  └── All shared projects as NuGet packages
  └── Each service in own repository
  └── Strict versioning and CI/CD per package
```

`★ Insight ─────────────────────────────────────`
**Pro tip:** Start with project references and migrate to NuGet only when you hit scaling pain points. Premature package extraction adds overhead without benefits. The key trigger is usually when **multiple teams** need to work independently on different services.
`─────────────────────────────────────────────────`

---

## Related Documents

- [gRPC Communication](./grpc-communication.md) - Proto definitions and patterns
- [Dual-Protocol Communication](./dual-protocol-communication.md) - gRPC/HTTP switching
- [Messaging Communication](./messaging-communication.md) - Outbox/Inbox patterns
- [Unit Testing](./unit-testing.md) - Testing with InMemory DbContext
