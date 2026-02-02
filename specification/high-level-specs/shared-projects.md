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
- **ServiceClients abstract gRPC clients** - client abstraction for inter-service communication
- **Common split by layer** - Api, Application, Infrastructure for clean separation

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
    ┌──────────────────┐  ┌─────────────────┐  ┌──────────────────────────┐
    │  EShop.Contracts │  │   EShop.Grpc    │  │    EShop.Common.*        │
    │  (Events, DTOs)  │  │   (Protos)      │  │  (Api, Application,      │
    └────────┬─────────┘  └────────┬────────┘  │   Infrastructure)        │
             │                     │           └─────────┬────────────────┘
             └──────────────┬──────┴─────────────────────┘
                            │
                            ▼
                 ┌─────────────────────────┐
                 │  EShop.ServiceClients   │
                 │  (gRPC clients)         │
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

**Structure:**
```
EShop.SharedKernel/
├── Domain/
│   ├── Entity.cs              # Base entity with Id and domain events
│   ├── AggregateRoot.cs       # Aggregate root marker + event collection
│   ├── ValueObject.cs         # Value object with equality
│   ├── IAggregateRoot.cs      # Marker interface
│   └── IOwnedEntity.cs        # Owned entity marker
├── Events/
│   ├── IDomainEvent.cs        # Domain event interface
│   └── DomainEventBase.cs     # Base record with timestamp
├── Services/
│   └── IDateTimeProvider.cs   # DateTime abstraction for testing
└── Guards/
    └── Guard.cs               # Guard clauses (Against.Null, etc.)
```

**Key Components:**
- `Entity<TId>` - Base entity with identity and domain events collection
- `AggregateRoot<TId>` - Aggregate root with version for optimistic concurrency
- `ValueObject` - Base value object with equality by components
- `Guard.Against.*` - Defensive programming guard clauses
- `IDateTimeProvider` - Abstraction for testable date/time operations

---

### 3.2 EShop.Contracts

**Purpose:** Integration events and shared DTOs for cross-service communication.

**NuGet Dependencies:** `EShop.SharedKernel` (optional, for IDomainEvent if needed)

**Location:** `src/Common/EShop.Contracts/`

**Structure:**
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
│   └── Product/
└── Constants/
    └── EventNames.cs             # Constants for event routing
```

**Key Components:**
- `IntegrationEvent` - Base record with EventId, Timestamp (CorrelationId via headers)
- Order events: `OrderConfirmed`, `OrderRejected`, `OrderCancelled`
- Product events: `StockLow`, `ProductCreated`, `ProductUpdated`, `StockReservationExpired`

---

### 3.3 EShop.Grpc

**Purpose:** Proto definitions and generated C# code for gRPC communication.

**NuGet Dependencies:** `Grpc.Tools`, `Google.Protobuf`, `Grpc.Net.Client`

**Location:** `src/Common/EShop.Grpc/`

**Structure:**
```
EShop.Grpc/
├── Protos/
│   └── product.proto
└── EShop.Grpc.csproj
```

**Key Points:**
- `GrpcServices="Both"` generates both client and server code
- Proto uses `csharp_namespace` for proper namespace
- Server (Product Service) needs `ProductService.ProductServiceBase`
- Client (Order Service via ServiceClients) needs `ProductService.ProductServiceClient`
- Keeps proto in one place, avoids version mismatch

---

### 3.4 EShop.Common.* (Split by Layer)

The Common project is split into three layers for clean separation:

#### 3.4.1 EShop.Common.Application

**Purpose:** CQRS infrastructure, MediatR behaviors, exceptions.

**Location:** `src/Common/EShop.Common.Application/`

**Structure:**
```
EShop.Common.Application/
├── Cqrs/
│   ├── ICommand.cs
│   └── IQuery.cs
├── Behaviors/
│   ├── ValidationBehavior.cs      # FluentValidation integration
│   ├── LoggingBehavior.cs         # Request/response logging
│   ├── UnitOfWorkBehavior.cs      # Transaction handling
│   ├── CommandTrackingBehavior.cs # Command tracking
│   └── DomainEventDispatchHelper.cs
├── Correlation/
│   ├── ICorrelationIdAccessor.cs
│   ├── CorrelationIdAccessor.cs   # HttpContext-based
│   ├── CorrelationContext.cs
│   └── CorrelationIdConstants.cs
└── Exceptions/
    ├── ApplicationException.cs
    ├── NotFoundException.cs
    ├── ValidationException.cs
    └── ConflictException.cs
```

#### 3.4.2 EShop.Common.Api

**Purpose:** HTTP middleware, gRPC interceptors, API layer utilities.

**Location:** `src/Common/EShop.Common.Api/`

**Structure:**
```
EShop.Common.Api/
├── Grpc/
│   ├── CorrelationIdServerInterceptor.cs
│   ├── GrpcExceptionInterceptor.cs
│   ├── GrpcValidationInterceptor.cs
│   └── GrpcLoggingInterceptor.cs
├── Http/
│   ├── GlobalExceptionHandler.cs
│   └── CorrelationIdMiddleware.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

#### 3.4.3 EShop.Common.Infrastructure

**Purpose:** Data access utilities, EF Core configurations.

**Location:** `src/Common/EShop.Common.Infrastructure/`

**Structure:**
```
EShop.Common.Infrastructure/
├── Data/
│   ├── EntityConfiguration.cs
│   ├── AggregateRootConfiguration.cs
│   └── RemoveEntitySuffixConvention.cs
├── Correlation/
│   └── (correlation infrastructure)
└── Extensions/
    └── (extension methods)
```

---

### 3.5 EShop.ServiceClients

**Purpose:** gRPC client abstraction for inter-service communication.

**NuGet Dependencies:**
- `EShop.Grpc`
- `EShop.Common.Application`
- `Grpc.Net.Client`
- `Grpc.Net.ClientFactory`
- `Riok.Mapperly` (code-gen mapping)
- `Microsoft.Extensions.ServiceDiscovery`

**Location:** `src/Common/EShop.ServiceClients/`

**Structure:**
```
EShop.ServiceClients/
├── Clients/
│   └── Product/
│       ├── GrpcProductServiceClient.cs
│       └── Mappers/
│           ├── GetProductsResponseMapper.cs
│           ├── ReserveStockRequestMapper.cs
│           ├── ReleaseStockRequestMapper.cs
│           ├── StockReservationResultMapper.cs
│           └── StockReleaseResultMapper.cs
├── Infrastructure/
│   └── Grpc/
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

**Key Components:**
- `IProductServiceClient` - Interface for product service operations
- `GrpcProductServiceClient` - gRPC implementation using generated client
- Mapperly mappers - Compile-time generated request/response mapping
- Service discovery integration for Aspire

See [gRPC Communication](./grpc-communication.md) for implementation details.

---

### 3.6 EShop.RoslynAnalyzers

**Purpose:** Custom Roslyn code analyzers for enforcing architectural rules.

**Location:** `src/Common/EShop.RoslynAnalyzers/`

**Key Features:**
- Compile-time code analysis
- Architectural rule enforcement
- Custom diagnostics and code fixes

---

## 4. Data Access Pattern

### 4.1 NO Repository Pattern

We use **direct DbContext access via interface** instead of Repository pattern.

**Rationale:**
- Simpler - no extra abstraction layer
- Full EF Core power - LINQ, Include, projections, raw SQL
- Interface in Application layer - still testable
- Less code to maintain

### 4.2 Interface in Application Layer

Each service defines its DbContext interface in the **Application layer**.

**Example structure:**
```
Order.Application/
└── Data/
    └── IOrderDbContext.cs    # Interface definition

Order.Infrastructure/
└── Data/
    └── OrderDbContext.cs     # Implementation
```

**Interface defines:**
- `DbSet<T>` properties for each entity
- `SaveChangesAsync` method

### 4.3 Registration Pattern

Infrastructure layer registers both the concrete DbContext and the interface:
- `AddDbContext<TContext>` registers the concrete type
- `AddScoped<IContext>` registers the interface pointing to concrete

### 4.4 Usage in Handlers

Handlers inject `IOrderDbContext` (or `IProductDbContext`) and use LINQ directly:
- Full EF Core capabilities available
- Projections for efficient queries
- Includes for navigation properties
- Tracking/no-tracking as needed

### 4.5 Testing

For unit tests:
- **EF Core InMemory** - Real LINQ, easy setup
- **Mock DbContext** - Full control for simple cases

For integration tests:
- **Testcontainers** with real PostgreSQL

---

## 5. Summary Table

| Project | Purpose | Dependencies | Key Contents |
|---------|---------|--------------|--------------|
| **SharedKernel** | DDD building blocks | NONE | Entity, AggregateRoot, ValueObject, Guards |
| **Contracts** | Cross-service contracts | SharedKernel (optional) | IntegrationEvents, Shared DTOs |
| **Grpc** | gRPC contracts | Grpc.Tools, Protobuf | Proto files, generated client/server |
| **Common.Application** | CQRS infrastructure | SharedKernel, MediatR | Behaviors, Exceptions, Correlation |
| **Common.Api** | API layer | Common.Application | Middleware, Interceptors |
| **Common.Infrastructure** | Data utilities | Common.Application, EF Core | Entity configs, conventions |
| **ServiceClients** | gRPC abstraction | Grpc, Common | IProductServiceClient, mappers |
| **ServiceDefaults** | Aspire config | Aspire | OpenTelemetry, Health checks |
| **RoslynAnalyzers** | Code analysis | Roslyn | Custom analyzers |

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
| **Common.*** | `EShop.Common.*` | SemVer | Infrastructure utilities |
| **ServiceClients** | `EShop.ServiceClients` | SemVer | Protocol abstractions |

### 6.3 Versioning Guidelines

| Change Type | Version Bump | Example |
|-------------|--------------|---------|
| Bug fix, no API change | PATCH | 1.0.0 → 1.0.1 |
| New feature, backward compatible | MINOR | 1.0.1 → 1.1.0 |
| Breaking change | MAJOR | 1.1.0 → 2.0.0 |

**Breaking changes in Contracts require coordination:**
1. Publish new version with `[Obsolete]` attributes
2. Update all consumers
3. Remove obsolete code in next major version

### 6.4 Migration Path

```
Phase 1: Current (Project References)
  └── All projects in single solution
  └── Direct references

Phase 2: Hybrid (Gradual Migration)
  └── SharedKernel + Contracts as NuGet
  └── Services still use project references for Common/ServiceClients

Phase 3: Full NuGet (Multi-Repo)
  └── All shared projects as NuGet packages
  └── Each service in own repository
  └── Strict versioning and CI/CD per package
```

**Recommendation:** Start with project references and migrate to NuGet only when you hit scaling pain points. The key trigger is usually when **multiple teams** need to work independently on different services.

---

## Related Documents

- [gRPC Communication](./grpc-communication.md) - Proto definitions and patterns
- [Messaging Communication](./messaging-communication.md) - Outbox/Inbox patterns
- [Unit Testing](./unit-testing.md) - Testing with InMemory DbContext
