# Architecture

System architecture and key design decisions.

## High-Level Overview

```
                      ┌──────────────────────┐
                      │     API Gateway      │
                      │    (YARP Proxy)      │
                      └──────────┬───────────┘
                                 │
                         ┌───────┴───────┐
                         ▼               ▼
                  ┌────────────┐  ┌────────────┐
                  │  Product   │  │   Order    │
                  │  Service   │  │  Service   │
                  └──────┬─────┘  └──────┬─────┘
                         │  gRPC (stock) │
                         │◄──────────────┤
                         │               │
                         │    Events     │
                         │──(catalog)───►│
                         │               │
                         ▼               ▼
                  ┌──────────┐    ┌──────────┐
                  │PostgreSQL│    │PostgreSQL│
                  │ + Outbox │    │ + Outbox │
                  └────┬─────┘    └─────┬────┘
                       │                │
                       └───────┬────────┘
                               ▼
                       ┌──────────────┐
                       │   RabbitMQ   │
                       └──────┬───────┘
                              │
            ┌─────────────────┼─────────────────┐
            ▼                 ▼                 ▼
     ┌────────────┐    ┌────────────┐    ┌────────────┐
     │Notification│    │ Analytics  │    │   Other    │
     └────────────┘    └────────────┘    └────────────┘
```

## Services

| Service | Domain | Key Features |
|---------|--------|--------------|
| **Product** | Catalog + Inventory | Product CRUD, stock management, gRPC server, publishes catalog events |
| **Order** | Order Lifecycle | Order placement, state machine, gRPC client (stock), event consumer (catalog) |
| **Notification** | Customer Communication | Email notifications via domain events |
| **Analytics** | Business Intelligence | Event aggregation, metrics tracking |
| **Gateway** | Request Routing | YARP proxy, rate limiting, correlation ID |
| **DatabaseMigration** | Schema Management | EF Core migrations at startup |

## Clean Architecture

Each service follows four-layer architecture:

```
┌─────────────────────────────────────────────┐
│                    API                       │  ← Controllers, minimal logic
├─────────────────────────────────────────────┤
│               Infrastructure                 │  ← EF Core, MassTransit, gRPC
├─────────────────────────────────────────────┤
│                Application                   │  ← Commands, Queries, Handlers
├─────────────────────────────────────────────┤
│                  Domain                      │  ← Entities, Value Objects, Events
└─────────────────────────────────────────────┘
```

**Dependency rule:** Dependencies point inward only. Domain has zero external dependencies.

## DDD Building Blocks

Implemented in `EShop.SharedKernel`:

- **Entity** - Objects with identity and lifecycle
- **Aggregate Root** - Consistency boundary, entry point to aggregate
- **Value Object** - Immutable objects defined by attributes
- **Domain Event** - Captures significant domain occurrences

## CQRS with MediatR

Commands and queries separated via MediatR handlers.

**Command flow:**
```
Controller → Command → Handler → Domain → DB → Domain Events → Integration Events
```

**Query flow:**
```
Controller → Query → Handler → DB (projection)
```

**Pipeline behaviors:** Logging → Validation → UnitOfWork → Handler

## Communication Patterns

### Synchronous: gRPC (Stock Operations)

Used for stock operations where immediate response is required.

```
Order Service ──ReserveStock──► Product Service
              ◄──Response────
              ──ReleaseStock──►
              ◄──Response────
```

### Asynchronous: Integration Events (Catalog Data)

Product catalog data (name, price) is synced via events to a local read model (`ProductSnapshot`) in the Order service. This decouples Order from Product for catalog lookups.

```
Product Service ──ProductChangedEvent──► RabbitMQ ──► Order Service (ProductSnapshot)
                                                   ──► Notification Service
                                                   ──► Analytics Service
```

**Materialized View: ProductSnapshot**

The Order service maintains a `ProductSnapshot` table — a local read model of product catalog data synced via `ProductChangedEvent`. When creating an order, the handler reads product name and price from this local table instead of making a synchronous gRPC call.

- Consumers use upsert + timestamp guard for natural idempotency
- On cold start, a background sync job populates initial data from Product service
- Eventual consistency is acceptable for catalog data (price captured at order time)

### Order Integration Events

```
Order Service ──OrderConfirmedEvent──► RabbitMQ ──► Notification Service
                                                ──► Analytics Service
```

**Fat events:** Events carry all necessary data to avoid consumer queries.

## Reliability Patterns

### Outbox Pattern (Publisher)

Events saved in same transaction as entity changes. Background job publishes to message broker.

### Inbox Pattern (Consumer)

Message ID stored on first processing. Duplicate messages acknowledged without reprocessing.

### Distributed Tracing

CorrelationId flows through HTTP headers, gRPC metadata, and message headers. All logs include correlation ID for request tracing.

## Key Decisions

| Decision | Rationale |
|----------|-----------|
| **Hybrid communication** | Catalog data via events (decoupled), stock ops via gRPC (real-time) |
| **ProductSnapshot read model** | Order service reads catalog data locally, resilient to Product service downtime |
| **Fat events** | Consumers don't need to query back for data |
| **No repository pattern** | Simplicity over testability for this demo |
| **gRPC for stock checks** | Immediate feedback needed for UX |
| **Aspire orchestration** | Native .NET integration, excellent DX |
