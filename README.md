# EShop Demo

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cooper538_eshop-demo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=cooper538_eshop-demo)

> Demonstrational microservices project
> Clean Architecture + DDD + CQRS
> Built with Specification-Driven AI Development

## What is this?

Reference implementation of .NET 10 microservices showcasing modern architectural patterns
and AI-assisted development methodology.

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 10, ASP.NET Core |
| Orchestration | .NET Aspire 9 |
| Database | PostgreSQL + EF Core 10 |
| Messaging | MassTransit + RabbitMQ |
| RPC | gRPC |
| API Gateway | YARP |
| Resilience | Polly 8.x |

## Quick Start

```bash
# Prerequisites: .NET 10 SDK, Docker

dotnet run --project src/AppHost
# Aspire Dashboard opens automatically with links to all services
```

## Architecture

```
                    ┌─────────────────┐
                    │   API Gateway   │  ← YARP reverse proxy
                    └────────┬────────┘
                             │ HTTP/REST
                        ┌────┴────┐
                        ▼         ▼
                    ┌───────┐   ┌───────┐
                    │Product│◄─►│ Order │
                    └───┬───┘   └───┬───┘
                        │  gRPC     │
                        └─────┬─────┘
                              │ Integration Events
                              ▼
                        ┌──────────┐
                        │ RabbitMQ │
                        └────┬─────┘
                             ▼
           ┌────────────┐         ┌───────────┐
           │Notification│         │ Analytics │
           └────────────┘         └───────────┘
```

### Services

| Service | Responsibility | Communication |
|---------|---------------|---------------|
| **Product** | Catalog management, inventory | REST API + gRPC server |
| **Order** | Order lifecycle, checkout | REST API + gRPC client |
| **Notification** | Event-driven email notifications | MassTransit consumer |
| **Analytics** | Business metrics aggregation | MassTransit consumer |
| **Gateway** | Request routing, rate limiting | YARP reverse proxy |
| **DatabaseMigration** | EF Core migrations at startup | Background worker |

## Key Patterns Demonstrated

### Architecture
- **Clean Architecture** - Domain → Application → Infrastructure → API layers
- **DDD** - Aggregates, entities, value objects, domain events
- **CQRS** - Separate command/query handlers via MediatR

### Reliability
- **Outbox Pattern** - Reliable event publishing with transactional guarantees
- **Inbox Pattern** - Idempotent message consumption, exactly-once processing
- **Distributed Tracing** - CorrelationId propagation across HTTP, gRPC, messaging

### Communication
- **Synchronous** - gRPC for Order → Product stock checks
- **Asynchronous** - MassTransit + RabbitMQ for integration events

## Project Structure

```
src/
├── AppHost/                      # .NET Aspire orchestration
├── ServiceDefaults/              # Shared Aspire configuration
├── Common/
│   ├── EShop.Common.Api/         # Shared API utilities
│   ├── EShop.Common.Application/ # Shared application layer
│   ├── EShop.Common.Infrastructure/ # Shared infrastructure
│   ├── EShop.Contracts/          # Integration events, shared DTOs
│   ├── EShop.Grpc/               # Proto definitions
│   ├── EShop.RoslynAnalyzers/    # Custom Roslyn analyzers
│   ├── EShop.ServiceClients/     # gRPC client abstraction
│   └── EShop.SharedKernel/       # DDD building blocks (zero deps)
└── Services/
    ├── Analytics/                # Business analytics
    ├── DatabaseMigration/        # Database migration worker
    ├── Gateway/                  # YARP API Gateway
    ├── Notification/             # Notification worker
    ├── Order/                    # Order lifecycle
    └── Products/                 # Product catalog + inventory
```

## Specification-Driven AI Development

This project demonstrates a novel approach where **AI acts as senior developer
guided by structured specifications**.

Instead of ad-hoc coding, each feature follows:
```
Specification → AI Implementation → Human Review → Commit
```

**Key benefits:**
- Consistent architecture across entire codebase
- Full traceability from specs to code
- Reproducible development workflow

## Deployment

Infrastructure as Code using Azure Bicep:

```
infra/
├── main.bicep              # Entry point
└── modules/
    ├── container-apps.bicep    # Azure Container Apps
    ├── postgres.bicep          # PostgreSQL Flexible Server
    ├── rabbitmq.bicep          # RabbitMQ on Container Apps
    └── ...
```

Deploy via GitHub Actions (CI/CD) or manually with Azure CLI.
Code quality analysis via SonarCloud (see badge above).

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture](docs/architecture.md) | System patterns and design decisions |
| [Deployment](docs/deployment.md) | Azure deployment with Bicep |
| [Spec-Driven Development](docs/spec-driven-development.md) | AI-assisted development methodology |
| [Code Guidelines](docs/code-guidelines.md) | Project-specific C# standards |
| [Aspire Integration](docs/aspire-integration.md) | Service orchestration guide |
| [Testing](docs/testing.md) | Test infrastructure and conventions |
| [Troubleshooting](docs/troubleshooting.md) | Common issues and solutions |

## Intentionally Simplified

This is a demonstration project focused on architectural patterns:

- **Authentication/authorization** - simplified for demo purposes
- **Test coverage** - not 100%, tests demonstrate patterns on Order Service only
- **Error handling** - basic implementation, production would need more robust handling
- **Infrastructure** - single region, no geo-redundancy or WAF
- **Deployment** - basic CI/CD, no release management
- **API versioning** - no versioning strategy implemented
- **Shared libraries** - project references instead of NuGet packages

## Commands

```bash
# Build
dotnet build EShopDemo.sln

# Test
dotnet test EShopDemo.sln

# Format
dotnet csharpier format .

# Run all services
dotnet run --project src/AppHost
```

## License

MIT

---

*Special thanks to Max Opus*
