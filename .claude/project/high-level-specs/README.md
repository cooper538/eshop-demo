# E-Shop Demo Microservices – Specification

## Metadata

| Attribute | Value |
|-----------|-------|
| Project Name | E-Shop Demo Microservices |
| Purpose | Demonstration of .NET microservices architecture |
| Technologies | .NET 10, ASP.NET Core, gRPC, MassTransit, RabbitMQ, PostgreSQL, Docker |
| Architectural Style | gRPC sync communication, Event-driven notifications, CQRS |

---

## 1. Project Overview

A demonstration project showcasing microservices architecture in .NET. The goal is to demonstrate depth of knowledge on a minimal functional scope, where each feature provides unique demonstration value.

### 1.1 Key Principles

- gRPC for synchronous communication between services (HTTP/2, Protocol Buffers)
- Event-driven architecture for notifications (MassTransit, RabbitMQ)
- Synchronous order processing with immediate response
- Outbox pattern for reliable integration event publishing
- Inbox pattern for idempotent notification processing

---

## 2. Architecture

```
┌─────────────────┐
│   API Gateway   │  ← YARP reverse proxy
└────────┬────────┘
         │ HTTP/REST (entry point only)
    ┌────┴────┐
    ▼         ▼
┌───────┐     ┌───────┐
│Product│◄───►│ Order │
└───┬───┘gRPC └───┬───┘
    │             │
    └──────┬──────┘
           │ Integration Events
           ▼
     ┌──────────┐
     │ RabbitMQ │
     └────┬─────┘
          │ publish/subscribe
          ▼
    ┌────────────┐
    │Notification│
    └────────────┘
```

### 2.1 Services

| Service | Type | Port | Database | Description |
|---------|------|------|----------|-------------|
| API Gateway | ASP.NET Core Web API | 5000 | – | YARP reverse proxy, rate limiting |
| Product Service | ASP.NET Core Web API + gRPC | 5001 (HTTP), 5051 (gRPC) | PostgreSQL | Product catalog, inventory management |
| Order Service | ASP.NET Core Web API + gRPC | 5002 (HTTP), 5052 (gRPC) | PostgreSQL | Order lifecycle, Internal gRPC API |
| Notification Service | Worker Service | – | – | Sending notifications (simulated) |

### 2.2 Communication Patterns

- **Order ↔ Product**: gRPC (synchronous, HTTP/2, Protocol Buffers)
- **Order → Notification**: Integration Events (asynchronous, MassTransit/RabbitMQ)
- **Product → Notification**: Integration Events (asynchronous, MassTransit/RabbitMQ)

---

## 3. Detailed Specifications

Architecture details are organized into focused specification documents:

### Shared Infrastructure

| Document | Description |
|----------|-------------|
| [Shared Projects](./shared-projects.md) | SharedKernel, Contracts, gRPC, Common, ServiceClients structure |
| [Error Handling](./error-handling.md) | HTTP error responses, ProblemDetails (RFC 7807), exception mapping |

### Service Interfaces

| Document | Description |
|----------|-------------|
| [Product Service Interface](./product-service-interface.md) | Product Service contracts (External + Internal API, stock operations) |
| [Order Service Interface](./order-service-interface.md) | Order Service contracts (Public API, Internal API dependencies) |

### Communication Patterns

| Document | Description |
|----------|-------------|
| [Internal API Communication](./internal-api-communication.md) | Internal API layer concept (/internal/*, security, routing) |
| [gRPC Communication](./grpc-communication.md) | gRPC technical patterns (server/client, resiliency, observability) |
| [Dual-Protocol Communication](./dual-protocol-communication.md) | Protocol abstraction (gRPC/HTTP switching, EShop.ServiceClients) |
| [Messaging Communication](./messaging-communication.md) | Integration events, MassTransit, Outbox/Inbox patterns |
| [CorrelationId Flow](./correlation-id-flow.md) | Distributed tracing across HTTP, gRPC, and messaging |

### Testing

| Document | Description |
|----------|-------------|
| [Unit Testing](./unit-testing.md) | Testing philosophy, domain/handler testing, TestServerCallContext |
| [Functional Testing](./functional-testing.md) | WebApplicationFactory, Testcontainers, Respawn, WireMock |

### DevOps & Infrastructure

| Document | Description |
|----------|-------------|
| [Aspire Orchestration](./aspire-orchestration.md) | Local dev orchestration, service discovery, Docker Compose |

---

## 4. Technology Stack

| Category | Technology | Version |
|----------|------------|---------|
| Framework | .NET | 10.0 |
| Web API | ASP.NET Core | 10.0 |
| RPC | Grpc.AspNetCore | 2.x |
| Messaging | MassTransit | 8.x |
| Message Broker | RabbitMQ | 3.x |
| Database | PostgreSQL | 16 |
| ORM | Entity Framework Core | 10.0 |
| API Gateway | YARP | 2.x |
| Validation | FluentValidation | 11.x |
| Mediator | MediatR | 12.x |
| Resilience | Polly | 8.x |
| Testing | xUnit, Moq, FluentAssertions, Testcontainers | latest |
| Containerization | Docker, Docker Compose | latest |
| Orchestration | .NET Aspire | 9.0 |
| CI/CD | GitHub Actions | – |

---

## 5. Demonstration Value

| Concept | Implementation |
|---------|----------------|
| gRPC | Synchronous Order ↔ Product communication (HTTP/2, Protocol Buffers) |
| Integration Events | Asynchronous notifications via MassTransit |
| CQRS | Separated Commands and Queries with MediatR |
| Outbox Pattern | Guaranteed integration event publishing |
| Inbox Pattern | Idempotent consumers in Notification service |
| State Machine | Order lifecycle management |
| Resilience | Polly retry, circuit breaker, gRPC deadlines |
| Clean Architecture | Domain, Application, Infrastructure layers |
| Domain-Driven Design | Aggregates, Domain Events, Value Objects |
| Distributed Tracing | CorrelationId propagation |
| Health Checks | Kubernetes-ready probes |
| API Gateway | YARP with rate limiting |
| Docker Orchestration | Multi-container setup with docker-compose |
| External API Integration | Typed HttpClient, Polly resilience (SendGrid) |

---

## 6. Repository Structure

```
eshop-demo/
├── .github/
│   ├── workflows/
│   └── dependabot.yml
├── src/
│   ├── AppHost/                    # Aspire orchestrator
│   │   └── EShop.AppHost/
│   ├── ServiceDefaults/            # Shared Aspire configuration
│   │   └── EShop.ServiceDefaults/
│   ├── Common/
│   │   ├── EShop.SharedKernel/     # DDD building blocks (Entity, AggregateRoot, ValueObject) - ZERO deps
│   │   ├── EShop.Contracts/        # Integration events, shared DTOs
│   │   ├── EShop.Grpc/             # Proto definitions + generated code
│   │   ├── EShop.Common/           # Shared infrastructure (behaviors, middleware, exceptions)
│   │   └── EShop.ServiceClients/   # Dual-protocol client abstraction (gRPC/HTTP)
│   └── Services/
│       ├── Gateway/
│       ├── Product/
│       │   ├── Product.API/
│       │   ├── Product.Application/  # Commands, Queries, IProductDbContext
│       │   ├── Product.Domain/       # Entities, Value Objects
│       │   └── Product.Infrastructure/ # DbContext, EF configurations
│       ├── Order/
│       │   ├── Order.API/
│       │   ├── Order.Application/    # Commands, Queries, IOrderDbContext
│       │   ├── Order.Domain/
│       │   └── Order.Infrastructure/
│       └── Notification/
├── tests/
├── docker/
├── docs/
├── Directory.Build.props
├── Directory.Packages.props
└── EShopDemo.sln
```

**Key architectural decisions:**
- **NO Repository pattern** - direct DbContext access via interface (`IProductDbContext`, `IOrderDbContext`)
- **SharedKernel has ZERO dependencies** - pure DDD building blocks
- See [Shared Projects Spec](./shared-projects.md) for details

---

## 7. Running the Project

### Development (Aspire)

```bash
# Clone repository
git clone https://github.com/USERNAME/eshop-demo.git
cd eshop-demo

# Start all services with Aspire dashboard
dotnet run --project src/AppHost

# Access
# Aspire Dashboard: https://localhost:17225
# API Gateway: https://localhost:5000
# RabbitMQ Management: http://localhost:15672 (guest/guest)
```

### Production (Docker Compose)

```bash
# Generate docker-compose from Aspire
dotnet run --project src/AppHost -- publish --output-path ./docker

# Run with Docker
cd docker && docker-compose up -d
```

---

## 8. Known Limitations & Trade-offs

This section documents conscious architectural trade-offs made for demo simplicity.

### 8.1 Synchronous Stock Reservation

Order Service synchronously calls Product Service (gRPC) when creating an order.

**Trade-off:**
| Aspect | Impact |
|--------|--------|
| ✅ UX | Immediate response to user |
| ✅ Simplicity | Easier to implement and debug |
| ❌ Coupling | Order Service unavailable when Product Service is down |

**Edge Case:** If Order DB save fails after successful stock reservation, orphan reservation may occur.

**Mitigation:**
- Stock reservation TTL (15 min expiration) with background cleanup job
- See [Product Service Interface - Stock Reservation Expiration](./product-service-interface.md#6-stock-reservation-expiration)

**Production Consideration:**
For guaranteed consistency, consider implementing Saga pattern (choreography-based) where order creation is async and stock reservation happens via events.

### 8.2 Event Design - Pragmatic Approach

`OrderConfirmed` event includes `CustomerEmail` and `Items` directly (not just IDs).

**Rationale:**
- Email changes are rare (< 0.1% of orders affected)
- All notification consumers need this data
- Sync dependency on Order Service for every notification creates worse coupling than occasional stale email

This is documented as a **conscious trade-off** in [Messaging Communication - Event Design Philosophy](./messaging-communication.md#3-event-design-philosophy).

### 8.3 Summary

| Limitation | Mitigation | Production Solution |
|------------|------------|---------------------|
| Sync stock reservation | TTL + cleanup job | Saga pattern |
| Fat events for notifications | Documented trade-off | Accept or implement event versioning |
| No distributed tracing storage | CorrelationId in logs | Jaeger/Zipkin |

---

## 9. Future Extensions (Out of Scope)

These features are intentionally omitted but can be discussed:

| Feature | Reason | Production Solution |
|---------|--------|---------------------|
| Authentication/Authorization | Complex, distracts from architecture | JWT + policy-based auth |
| Kubernetes | Docker Compose sufficient for demo | K8s manifests, Helm charts |
| Distributed Caching | IMemoryCache sufficient | Redis |
| Event Sourcing | Over-engineering for demo | EventStoreDB |
| Service Mesh | Complexity | Istio, Linkerd |
| **NuGet Packages** | Single repo sufficient for demo | Internal NuGet feed (GitHub Packages, Azure Artifacts) |

### 9.1 NuGet Package Migration

When scaling to multiple teams/repos, shared projects can be extracted to internal NuGet packages:

```
Current: Project References          Future: NuGet Packages
─────────────────────────────        ─────────────────────────
<ProjectReference Include=           <PackageReference Include=
  "..\..\Common\EShop.SharedKernel     "EShop.SharedKernel"
  \EShop.SharedKernel.csproj" />       Version="1.2.0" />
```

See [Shared Projects Spec - NuGet Extension](./shared-projects.md#6-future-extension-nuget-packages) for detailed migration path.