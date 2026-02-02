# Testing

Testing infrastructure and conventions.

## Test Types

| Type | Project | Purpose | Infrastructure |
|------|---------|---------|----------------|
| **Unit** | `Common.UnitTests`, `Order.UnitTests` | Domain logic, handlers, validators | None (pure C#) |
| **Integration** | `Order.IntegrationTests` | EF Core, MassTransit, API endpoints | Testcontainers (PostgreSQL, RabbitMQ) |
| **E2E** | `E2E.Tests` | Full order flows across services | Aspire.Hosting.Testing |
| **Architecture** | `EShop.ArchitectureTests` | Layer dependencies, Clean Architecture rules | NetArchTest |

## Running Tests

```bash
# All tests
dotnet test EShopDemo.sln

# By project
dotnet test tests/Order.UnitTests
dotnet test tests/Order.IntegrationTests
```

## Test Stack

- **Framework**: xUnit
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Containers**: Testcontainers
- **Database Reset**: Respawn

Naming follows `Method_Scenario_ExpectedResult` pattern (best practice).

## Coverage Note

This is a **demonstration project** - test coverage is intentionally not 100%.

Tests focus on Order Service as it has the richest business logic:
- State machine transitions
- gRPC integration with Product service
- Domain events and messaging
- Transactional outbox pattern

The goal is to demonstrate testing patterns, not achieve full coverage.
