# Project Roadmap

Overview of all implementation phases for the EShop microservices demo.

## Progress

```
[###############-] 15/16 phases completed (94%)
```

## Phases

| # | Phase | Status |
|---|-------|--------|
| 01 | [Foundation](#phase-01-foundation-) | ✅ Done |
| 02 | [Aspire](#phase-02-aspire-) | ✅ Done |
| 03 | [Product Core](#phase-03-product-core-) | ✅ Done |
| 04 | [Product Internal](#phase-04-product-internal-) | ✅ Done |
| 05 | [Order Core](#phase-05-order-core-) | ✅ Done |
| 06 | [Order Integration](#phase-06-order-integration-) | ✅ Done |
| 07 | [Messaging](#phase-07-messaging-) | ✅ Done |
| 08 | [Notification](#phase-08-notification-) | ✅ Done |
| 09 | [Gateway](#phase-09-gateway-) | ✅ Done |
| 10 | [Testing & Validation](#phase-10-testing--validation-) | ✅ Done |
| 11 | [Improvements & Refactoring](#phase-11-improvements--refactoring-) | ✅ Done |
| 12 | [Azure App Adaptation](#phase-12-azure-app-adaptation-) | ✅ Done |
| 13 | [Azure Infrastructure](#phase-13-azure-infrastructure-) | ✅ Done |
| 14 | [Authentication](#phase-14-authentication-) | ✅ Done |
| 15 | [Order-Product Event Decoupling](#phase-15-order-product-event-decoupling-) | ✅ Done |
| 16 | [gRPC Server-Side Streaming](#phase-16-grpc-server-side-streaming-) | ⬜ Pending |

---

# Phase Details

---

## Phase 01: Foundation ✅

**Shared libraries and solution infrastructure**

- Create solution file `EShopDemo.sln`
- Create `Directory.Build.props` and `Directory.Packages.props` (central package management)
- Implement `EShop.SharedKernel` (Entity, AggregateRoot, ValueObject, IDomainEvent, Guard)
- Implement `EShop.Contracts` (integration events, service client interfaces)
- Implement `EShop.Grpc` (proto definitions for Product Service)
- Implement `EShop.Common.*` - layered approach:
  - `EShop.Common.Application` - exceptions, behaviors, correlation, CQRS
  - `EShop.Common.Api` - HTTP middleware, gRPC server interceptors
  - `EShop.Common.Infrastructure` - MassTransit filters, EF configurations
- Implement `EShop.ServiceClients` (gRPC client abstraction for internal API)

→ [Details](./phase-01-foundation/phase.md)

---

## Phase 02: Aspire ✅

**.NET Aspire orchestration and local development**

- Create `EShop.AppHost` (Aspire orchestrator)
- Create `EShop.ServiceDefaults` (shared configuration, health checks, OpenTelemetry)
- Configure PostgreSQL (3 databases) and RabbitMQ resources
- Set up service discovery and resilience
- Configure Serilog with CorrelationId support
- Add Docker Compose publishing support

→ [Details](./phase-02-aspire/phase.md)

---

## Phase 03: Product Core ✅

**Product Service with domain model, REST API, and gRPC API**

- Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- Implement Product aggregate (Name, Description, Price)
- Implement Stock aggregate (separate from Product for DDD alignment)
- Implement CQRS handlers (CreateProduct, GetProducts, GetProductById, UpdateProduct)
- Configure EF Core with PostgreSQL
- Create external REST API endpoints
- Create internal gRPC API (GetStock, ReserveStock, ReleaseStock)
- Add FluentValidation validators

→ [Details](./phase-03-product-core/phase.md)

---

## Phase 04: Product Internal ✅

**Internal gRPC API and stock management**

- Implement gRPC server (ProductGrpcService)
- Add StockReservation entity and logic
- Implement ReserveStock and ReleaseStock operations
- Implement stock reservation expiration (TTL cleanup)

→ [Details](./phase-04-product-internal/phase.md)

---

## Phase 05: Order Core ✅

**Order Service domain with lifecycle management and Product integration**

- Create Clean Architecture structure
- Implement Order aggregate with status transitions (Created → Confirmed/Rejected → Cancelled)
- Implement OrderItem as owned entity
- Create CQRS handlers (CreateOrder, GetOrder, ConfirmOrder, CancelOrder)
- Configure EF Core with PostgreSQL
- Create external REST API endpoints
- Integrate with Product Service via gRPC (stock reservation)
- Configure MassTransit for messaging

→ [Details](./phase-05-order-core/phase.md)

---

## Phase 06: Order Integration ✅

**gRPC communication with Product Service**

- Configure gRPC client in ServiceClients for Order Service
- Configure resilience policies (gRPC built-in retry with exponential backoff)
- Integrate stock reservation into CreateOrder flow
- Add CorrelationId propagation (HTTP middleware + gRPC interceptors)

→ [Details](./phase-06-order-integration/phase.md)

---

## Phase 07: Messaging ✅

**Event-driven communication with RabbitMQ**

- Configure MassTransit with RabbitMQ
- Configure MassTransit Bus Outbox (transactional outbox pattern)
- Implement Inbox pattern in EShop.Common for idempotent consumers
- Add integration event publishing (OrderConfirmed, OrderRejected, OrderCancelled, StockLowAlert)
- Publish domain events through MassTransit

→ [Details](./phase-07-messaging/phase.md)

---

## Phase 08: Notification ✅

**Worker service for notification processing**

- Create Worker Service project
- Implement event consumers (OrderConfirmedConsumer, StockLowConsumer, etc.)
- Implement Inbox pattern for idempotent processing
- Simulate email sending (logging instead of actual SendGrid)

→ [Details](./phase-08-notification/phase.md)

---

## Phase 09: Gateway ✅

**YARP reverse proxy as single entry point**

- Create Gateway project
- Configure YARP reverse proxy
- Set up routing to Product and Order services
- Add rate limiting
- Add CorrelationId middleware (generation for external requests)

→ [Details](./phase-09-gateway/phase.md)

---

## Phase 10: Testing & Validation ✅

**Demonstrational test coverage (~80%) for core infrastructure and Order Service - showcasing unit, integration, and E2E testing patterns**

- Unit test infrastructure and shared test utilities
- SharedKernel DDD building blocks tests
- Application behaviors tests (pipeline, correlation, domain events)
- Integration test infrastructure (Testcontainers, Respawn)
- Order Domain tests (state machine, entities)
- E2E test infrastructure (Aspire.Hosting.Testing)
- Order Application tests (handlers, validators)
- Order integration tests (API, DB, messaging)
- E2E order flow tests (including CorrelationId propagation)

→ [Details](./phase-10-validation/phase.md)

---

## Phase 11: Improvements & Refactoring ✅

**Technical debt, refactoring, domain model improvements, and E2E validation**

- Separate Product catalog from Stock inventory (distinct aggregates)
- Architecture tests for Clean Architecture and DDD compliance
- UnitOfWork behavior refactoring (domain events before SaveChanges)
- IDateTimeProvider abstraction for testability
- Analytics Service (pub-sub pattern demonstration)
- E2E happy flow validation (Order flows, Stock Low Alert, CorrelationId)
- E2E error flow validation (404, 400, service unavailable)

→ [Details](./phase-11-improvements/phase.md)

---

## Phase 12: Azure App Adaptation ✅

**Environment-aware configuration for Azure deployment**

- Add SSL mode handling for Azure PostgreSQL Flexible Server connections
- Integrate Azure Key Vault configuration provider with DefaultAzureCredential
- Configure gRPC clients for Container Apps service discovery (FQDN pattern)
- Update all services to use environment-aware configuration

→ [Details](./phase-12-azure-app-adaptation/phase.md)

---

## Phase 13: Azure Infrastructure ✅

**Infrastructure as Code (Bicep) and CI/CD pipelines for Azure Container Apps**

- Set up `infra/` folder structure with Bicep modules
- Create identity, monitoring, postgres, rabbitmq, key-vault modules
- Create container-apps.bicep (Environment + 5 Container Apps with GHCR)
- Create Dockerfiles for all services
- Create GitHub Actions workflows for infrastructure and application deployment (OIDC auth)

→ [Details](./phase-13-azure-infrastructure/phase.md)

---

## Phase 14: Authentication ✅

**JWT Bearer authentication at API Gateway level**

- Configure JWT Bearer authentication in Gateway
- Add authorization policies for YARP routes
- Document Azure AD (Entra ID) setup for token acquisition
- Security hardening (RS256 algorithm whitelist, HSTS)

→ [Details](./phase-14-authentication/phase.md)

---

## Phase 15: Order-Product Event Decoupling ✅

**Replace synchronous gRPC catalog lookup with event-driven local read model**

- Publish `ProductCreatedEvent` / `ProductUpdatedEvent` integration events from Product Service
- Create `ProductSnapshot` read model entity in Order Service
- Implement MassTransit consumers for product events (upsert with timestamp guard)
- Replace gRPC `GetProducts` call in `CreateOrderCommandHandler` with local `ProductSnapshot` query
- Add startup sync job for initial `ProductSnapshot` population (cold start)
- Update unit/integration tests for new data flow

→ [Details](./phase-15-order-product-decoupling/phase.md)

---

## Phase 16: gRPC Server-Side Streaming ⬜

**Convert `GetAllProducts` from unary to server-side streaming with constant memory usage**

- Change `GetAllProducts` proto from unary to `stream ProductInfo`
- Create `StreamAllProductsQuery` MediatR stream handler with `AsAsyncEnumerable()`
- Add streaming overrides to all server-side and client-side gRPC interceptors
- Update `ProductGrpcService.GetAllProducts` to streaming with `CreateStream()`
- Update `IProductServiceClient` to return `IAsyncEnumerable<ProductInfo>`
- Convert `ProductSnapshotSyncJob` to `await foreach` with batched saves
- Update unit tests and delete obsolete code

> [Details](./phase-16-grpc-streaming/phase.md)

---

## Architecture Flow

```
Phase 01-02: Infrastructure
    └── SharedKernel, Contracts, Aspire setup

Phase 03-04: Product Service
    └── Domain → Application → API (REST + gRPC)

Phase 05-06: Order Service
    └── Domain → Lifecycle → Integration with Product

Phase 07-08: Messaging
    └── RabbitMQ → Integration Events → Notification Worker

Phase 09-11: Gateway, Validation & Improvements
    └── YARP Gateway → E2E Tests → Analytics Service

Phase 12-14: Azure & Security
    └── App Adaptation → Bicep IaC → JWT Authentication

Phase 15: Event Decoupling
    └── Product Events → ProductSnapshot Read Model → Local Query

Phase 16: gRPC Streaming
    └── Server-Side Streaming → Constant Memory → Batched Client Saves
```

## Quick Commands

```bash
/task-status      # Check current status
/start-task XX    # Start next available task
/finish-task      # Complete current task
/finish-phase XX  # Complete a phase manually
```
