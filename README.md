# EShop Demo

Demonstration of .NET microservices architecture showcasing gRPC communication, event-driven patterns, and clean architecture principles.

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 10 |
| RPC | gRPC (Grpc.AspNetCore) |
| Messaging | MassTransit + RabbitMQ |
| Database | PostgreSQL + EF Core 10 |
| API Gateway | YARP |
| Orchestration | .NET Aspire 9 |
| Resilience | Polly 8.x |

## Architecture

```
┌─────────────────┐
│   API Gateway   │  ← YARP reverse proxy
└────────┬────────┘
         │ HTTP/REST
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
          ▼
    ┌────────────┐
    │Notification│
    └────────────┘
```

## Project Structure

```
src/
├── Common/
│   ├── EShop.SharedKernel/      # DDD building blocks (zero deps)
│   ├── EShop.Contracts/         # Integration events, shared DTOs
│   ├── EShop.Grpc/              # Proto definitions
│   ├── EShop.Common/            # Shared infrastructure
│   └── EShop.ServiceClients/    # gRPC client abstraction
└── Services/
    ├── Gateway/                 # YARP API Gateway
    ├── Product/                 # Product catalog, inventory
    ├── Order/                   # Order lifecycle
    └── Notification/            # Notification worker
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for RabbitMQ, PostgreSQL)

### Run with Aspire

```bash
dotnet run --project src/AppHost
```

The Aspire dashboard opens automatically with links to all services.

### Build & Test

```bash
# Build
dotnet build EShopDemo.sln

# Test
dotnet test EShopDemo.sln

# Format
dotnet csharpier format .
```

## Key Patterns

- **DDD** - Aggregates, Domain Events, Value Objects
- **Clean Architecture** - Domain → Application → Infrastructure → API
- **CQRS** - Separated Commands/Queries with MediatR
- **Outbox/Inbox** - Reliable event publishing and idempotent consumers
- **gRPC** - Synchronous inter-service communication

## Documentation

Detailed specs available in `specification/high-level-specs/`:
- Architecture overview
- Service interfaces
- Communication patterns
- Testing strategies

## License

MIT

---

*Special thanks to Max Opus*
