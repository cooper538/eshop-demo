# Project Roadmap

Overview of all implementation phases for the EShop microservices demo.

## Progress

```
[###-------] 3/10 phases completed (30%)
```

---

## Phase 01: Foundation ✅

**Shared libraries and solution infrastructure**

- Create solution file `EShopDemo.sln`
- Create `Directory.Build.props` and `Directory.Packages.props` (central package management)
- Implement `EShop.SharedKernel` (Entity, AggregateRoot, ValueObject, IDomainEvent, Guard)
- Implement `EShop.Contracts` (integration events, shared DTOs)
- Implement `EShop.Grpc` (proto definitions for Product Service)
- Implement `EShop.Common` (middleware, behaviors, exception handling)
- Implement `EShop.ServiceClients` (interface abstraction for dual-protocol)

→ [Details](./phase-01-foundation/phase.md)

---

## Phase 02: Aspire ✅

**.NET Aspire orchestration and local development**

- Create `EShop.AppHost` (Aspire orchestrator)
- Create `EShop.ServiceDefaults` (shared configuration, health checks, OpenTelemetry)
- Configure PostgreSQL and RabbitMQ resources
- Set up service discovery

→ [Details](./phase-02-aspire/phase.md)

---

## Phase 03: Product Core ✅

**Product Service domain model and external REST API**

- Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- Implement domain entities (Product, Category)
- Implement CQRS handlers (CreateProduct, GetProducts, GetProductById, UpdateProduct)
- Configure EF Core with PostgreSQL
- Create external REST API endpoints
- Add FluentValidation validators
- Configure YAML-based settings with schema validation

→ [Details](./phase-03-product-core/phase.md)

---

## Phase 04: Product Internal ⚪

**Internal gRPC API and stock management**

- Implement gRPC server (ProductGrpcService)
- Add StockReservation entity and logic
- Implement ReserveStock and ReleaseStock operations
- Add internal REST endpoints (`/internal/*`)
- Implement stock reservation expiration (TTL cleanup)

→ [Details](./phase-04-product-internal/phase.md)

---

## Phase 05: Order Core ⚪

**Order Service domain with state machine**

- Create Clean Architecture structure
- Implement Order entity with state machine (Pending → Confirmed/Rejected → Cancelled)
- Implement OrderItem value object
- Create CQRS handlers (CreateOrder, GetOrder, CancelOrder)
- Configure EF Core with PostgreSQL
- Create external REST API endpoints

→ [Details](./phase-05-order-core/phase.md)

---

## Phase 06: Order Integration ⚪

**Dual-protocol communication with Product Service**

- Implement `GrpcProductServiceClient` in ServiceClients
- Implement `HttpProductServiceClient` in ServiceClients
- Configure Polly resilience policies (retry, circuit breaker)
- Integrate stock reservation into CreateOrder flow
- Add CorrelationId propagation (middleware + interceptors)

→ [Details](./phase-06-order-integration/phase.md)

---

## Phase 07: Messaging ⚪

**Event-driven communication with RabbitMQ**

- Configure MassTransit with RabbitMQ
- Implement Outbox pattern in Order Service
- Implement Inbox pattern in EShop.Common
- Create OutboxProcessor background service
- Add integration event publishing (OrderConfirmed, OrderRejected, OrderCancelled)

→ [Details](./phase-07-messaging/phase.md)

---

## Phase 08: Notification ⚪

**Worker service for notification processing**

- Create Worker Service project
- Implement event consumers (OrderConfirmedConsumer, StockLowConsumer, etc.)
- Implement Inbox pattern for idempotent processing
- Simulate email sending (logging instead of actual SendGrid)

→ [Details](./phase-08-notification/phase.md)

---

## Phase 09: Gateway ⚪

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
- Order Service domain + state machine tests
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

## Architecture Flow

```
Phase 01-02: Infrastructure
    └── SharedKernel, Contracts, Aspire setup

Phase 03-04: Product Service
    └── Domain → Application → API (REST + gRPC)

Phase 05-06: Order Service
    └── Domain → State Machine → Integration with Product

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
