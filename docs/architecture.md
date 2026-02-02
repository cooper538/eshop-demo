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
                         │    gRPC       │
                         │◄──────────────┤
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
| **Product** | Catalog + Inventory | Product CRUD, stock management, gRPC server |
| **Order** | Order Lifecycle | Order placement, state machine, gRPC client |
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

### Synchronous: gRPC

Used when immediate response is required (product lookup and stock checks before order placement).

```
Order Service ──GetProducts──► Product Service
              ◄──ProductInfo──
              ──ReserveStock──►
              ◄──Response────
```

The Order Service first fetches product information (name, price) via `GetProducts`, then reserves stock via `ReserveStock`. This ensures consistent pricing and validated product existence.

### Asynchronous: Integration Events

Used for eventual consistency and decoupling via MassTransit + RabbitMQ.

```
Order Service ──OrderCreatedEvent──► RabbitMQ ──► Notification Service
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
| **Fat events** | Consumers don't need to query back for data |
| **No repository pattern** | Simplicity over testability for this demo |
| **gRPC for stock checks** | Immediate feedback needed for UX |
| **Aspire orchestration** | Native .NET integration, excellent DX |
