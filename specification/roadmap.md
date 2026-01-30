# Project Roadmap

Overview of all implementation phases for the EShop microservices demo.

## Progress

```
[#########-] 9/11 phases completed (82%)
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
| 10 | [Testing & Validation](#phase-10-testing--validation-) | ⚪ Pending |
| 11 | [Improvements & Refactoring](#phase-11-improvements--refactoring-) | 🔵 In Progress |

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

## Phase 10: Testing & Validation ⚪

**Comprehensive testing across all layers and E2E validation**

### Unit Tests
- SharedKernel tests (Entity, ValueObject, Guard)
- EShop.Common tests (behaviors, middleware)
- Product Service domain + stock operations tests
- Order Service domain + lifecycle tests
- Notification consumers tests

### Integration Tests
- Product Service with mocked dependencies
- Order-Product integration with mocked Product Service
- MassTransit Test Harness tests

### Functional/E2E Tests
- WebApplicationFactory + Testcontainers setup
- Respawn for database cleanup
- Complete order flow (Gateway → Order → Product → Notification)
- CorrelationId propagation end-to-end
- Document project startup

→ [Details](./phase-10-validation/phase.md)

---

## Phase 11: Improvements & Refactoring 🔵

**Technical debt, refactoring, and domain model improvements**

This phase contains improvements and refactoring that emerged during development. These enhancements improve code quality and DDD alignment.

### Completed Tasks
- **Task 01: Product Domain Refactoring** - Separate Product catalog from Stock inventory into distinct aggregates
- **Task 02: Architecture Tests** - NetArchTest.Rules tests for Clean Architecture and DDD compliance
- **Task 03: UnitOfWork Behavior** - Refactor domain event dispatch to run before SaveChangesAsync
- **Task 04: IDateTimeProvider** - Introduce IDateTimeProvider abstraction for testability
- **Task 05: Analytics Service** - New microservice demonstrating pub-sub pattern

### Pending Tasks
- **Task 06: E2E Happy Flow Validation** - Complete E2E validation of all Order flows
- **Task 07: E2E Error Flow Validation** - Complete E2E validation of error flows

→ [Details](./phase-11-improvements/phase.md)

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

Phase 09-10: Gateway & Validation
    └── YARP Gateway → E2E Tests
```

## Quick Commands

```bash
/task-status      # Check current status
/start-task XX    # Start next available task
/finish-task      # Complete current task
/finish-phase XX  # Complete a phase manually
```
