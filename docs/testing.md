# Testing

Guide to testing infrastructure and conventions in EShop Demo.

## Test Projects

| Project | Type | Purpose |
|---------|------|---------|
| `Common.UnitTests` | Unit | SharedKernel DDD building blocks, application behaviors |
| `Order.UnitTests` | Unit | Order domain entities, application handlers, validators |
| `Order.IntegrationTests` | Integration | EF Core persistence, MassTransit messaging, API endpoints |
| `E2E.Tests` | E2E | Full order flows via Aspire.Hosting.Testing |
| `EShop.ArchitectureTests` | Architecture | Layer dependencies, naming conventions, Clean Architecture rules |

## Running Tests

```bash
# All tests
dotnet test EShopDemo.sln

# By project
dotnet test tests/Common.UnitTests
dotnet test tests/Order.UnitTests
dotnet test tests/Order.IntegrationTests
dotnet test tests/E2E.Tests
dotnet test tests/EShop.ArchitectureTests

# By category (if using traits)
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=E2E"

# With verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Test Infrastructure

### Unit Tests
- **Framework**: xUnit
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- No external dependencies required

### Integration Tests
- **Containers**: Testcontainers (PostgreSQL, RabbitMQ)
- **Database Reset**: Respawn for fast cleanup between tests
- **Fixtures**: Shared container instances via `ICollectionFixture<T>`

### E2E Tests
- **Orchestration**: Aspire.Hosting.Testing
- **Approach**: Spins up full application with all services
- **Fixtures**: `E2ETestFixture` manages Aspire application lifecycle

## Naming Conventions

### Test Classes
```
{Subject}Tests.cs
{Subject}Tests.{Scenario}.cs  // for partial classes
```

Examples:
- `OrderEntityTests.cs`
- `OrderEntityTests.Cancel.cs`
- `CreateOrderCommandHandlerTests.cs`

### Test Methods
```
{Method}_{Scenario}_{ExpectedResult}
{Method}_Should{ExpectedBehavior}_When{Condition}
```

Examples:
- `Create_WithValidData_ReturnsNewOrder`
- `Cancel_ShouldThrow_WhenOrderAlreadyShipped`
- `Handle_ReturnsOrders_WhenOrdersExist`

## Adding New Tests

### Unit Test
1. Create test class in appropriate project (`Common.UnitTests` or `Order.UnitTests`)
2. Inherit from base class if needed (e.g., for shared setup)
3. Use `[Fact]` for single cases, `[Theory]` with `[InlineData]` for parameterized tests

```csharp
public class MyServiceTests
{
    [Fact]
    public void MyMethod_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var sut = new MyService();

        // Act
        var result = sut.MyMethod("input");

        // Assert
        result.Should().Be("expected");
    }
}
```

### Integration Test
1. Create test class in `Order.IntegrationTests`
2. Inherit from `OrderIntegrationTestBase`
3. Use `[Collection(nameof(OrderIntegrationTestCollection))]` for shared containers

```csharp
[Collection(nameof(OrderIntegrationTestCollection))]
public class MyIntegrationTests : OrderIntegrationTestBase
{
    public MyIntegrationTests(OrderIntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MyTest()
    {
        // Arrange - use Fixture.DbContext, Fixture.Harness, etc.

        // Act

        // Assert
    }
}
```

### E2E Test
1. Create test class in `E2E.Tests`
2. Inherit from `E2ETestBase`
3. Use `[Collection(nameof(E2ETestCollection))]`

```csharp
[Collection(nameof(E2ETestCollection))]
public class MyE2ETests : E2ETestBase
{
    public MyE2ETests(E2ETestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MyE2ETest()
    {
        // Use HttpClient to call APIs
        var response = await HttpClient.GetAsync("/api/orders");
        response.Should().BeSuccessful();
    }
}
```

## Test Coverage Focus

This is a demonstrational project - tests focus on Order Service as it has the richest business logic:
- State machine transitions
- gRPC integration with Product service
- Domain events and messaging
- Transactional outbox pattern

Same testing patterns apply to other services.
