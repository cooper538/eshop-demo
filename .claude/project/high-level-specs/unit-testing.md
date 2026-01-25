# Unit Testing Architecture

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Unit testing strategy and patterns |
| Applies To | All services |
| Framework | xUnit, Moq, FluentAssertions |

---

## 1. Overview

Unit testing focuses on **core business logic** with representative tests for infrastructure code. The philosophy prioritizes meaningful coverage over metrics-driven exhaustive testing.

---

## 2. Testing Stack

| Tool | Purpose |
|------|---------|
| **xUnit** | Test framework |
| **Moq** | Mocking framework |
| **FluentAssertions** | Readable assertions |
| **AutoFixture** | Test data generation (optional) |

---

## 3. Testing Philosophy

### 3.1 What to Test Thoroughly

Focus unit testing effort on **core functionality**:

| Area | Priority | Examples |
|------|----------|----------|
| **Domain Entities** | High | `Order.Confirm()`, `Product.ReserveStock()` |
| **Value Objects** | High | `Money`, `Email`, `OrderId` validation |
| **Domain Services** | High | Complex business rules |
| **Status Transitions** | High | Order lifecycle methods |
| **MediatR Handlers** | High | Command/Query handlers |
| **Validators** | Medium | FluentValidation rules |

### 3.2 Representative Testing

For areas outside core logic, use **one representative test** to verify setup:

| Area | Representative Test |
|------|---------------------|
| Controllers | One test verifying routing and model binding |
| Validators | One test verifying FluentValidation integration |
| Mappers | One test verifying AutoMapper configuration |
| gRPC Services | One test verifying MediatR dispatch |

### 3.3 What NOT to Test

Skip unit tests for:
- Simple DTOs/models without logic
- Auto-generated code (EF migrations, gRPC stubs)
- Framework code (middleware setup, DI configuration)
- Trivial getters/setters

---

## 4. Test Structure

### 4.1 Naming Convention

```
{MethodName}_{Scenario}_{ExpectedResult}
```

Examples:
- `Confirm_WhenOrderIsCreated_ChangesStatusToConfirmed`
- `ReserveStock_WhenInsufficientQuantity_ReturnsFalse`
- `Handle_WithValidCommand_PublishesEvent`

### 4.2 AAA Pattern

```csharp
[Fact]
public async Task Handle_WithValidCommand_CreatesOrderAndReturnsSuccess()
{
    // Arrange
    var dbContext = CreateInMemoryDbContext(); // or Mock<IOrderDbContext>
    var productClient = new Mock<IProductServiceClient>();
    var outbox = new Mock<IOutboxRepository>();
    var handler = new CreateOrderCommandHandler(dbContext, productClient.Object, outbox.Object);
    var command = new CreateOrderCommand(/* ... */);

    productClient
        .Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(StockReservationResult.Success());

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.Should().Be(EOrderStatus.Confirmed);
    dbContext.Orders.Should().HaveCount(1);
}
```

---

## 5. Domain Entity Testing

### 5.1 State Transition Tests

```csharp
public class OrderTests
{
    [Fact]
    public void Confirm_WhenStatusIsCreated_ChangesStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new List<OrderItem>());

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(EOrderStatus.Confirmed);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WhenStatusIsNotCreated_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new List<OrderItem>());
        order.Confirm(); // Now Confirmed

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot confirm order in Confirmed state*");
    }

    [Fact]
    public void Cancel_WhenConfirmed_ChangesStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new List<OrderItem>());
        order.Confirm();

        // Act
        order.Cancel("Customer request");

        // Assert
        order.Status.Should().Be(EOrderStatus.Cancelled);
        order.RejectionReason.Should().Be("Customer request");
    }
}
```

### 5.2 Stock Operations Tests

```csharp
public class ProductTests
{
    [Fact]
    public void ReserveStock_WhenSufficientQuantity_DecreasesStockAndReturnsTrue()
    {
        // Arrange
        var product = new Product("Test", 100, 10); // 100 in stock, threshold 10

        // Act
        var result = product.ReserveStock(50);

        // Assert
        result.Should().BeTrue();
        product.StockQuantity.Should().Be(50);
    }

    [Fact]
    public void ReserveStock_WhenInsufficientQuantity_ReturnsFalseAndDoesNotChange()
    {
        // Arrange
        var product = new Product("Test", 10, 5);

        // Act
        var result = product.ReserveStock(20);

        // Assert
        result.Should().BeFalse();
        product.StockQuantity.Should().Be(10); // unchanged
    }

    [Fact]
    public void IsLowStock_WhenBelowThreshold_ReturnsTrue()
    {
        // Arrange
        var product = new Product("Test", 5, 10); // 5 in stock, threshold 10

        // Act & Assert
        product.IsLowStock.Should().BeTrue();
    }
}
```

---

## 6. MediatR Handler Testing

### 6.1 DbContext Testing Approaches

For handler tests, you have two options:

| Approach | When to Use | Pros | Cons |
|----------|-------------|------|------|
| **EF Core InMemory** | Most handler tests | Real LINQ, easy setup | Not 100% SQL compatible |
| **Mock DbContext** | Simple cases, isolation | Full control | Complex setup for DbSet |

### 6.2 InMemory DbContext Helper

```csharp
// tests/Order.UnitTests/Helpers/TestDbContextFactory.cs
public static class TestDbContextFactory
{
    public static OrderDbContext Create()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new OrderDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
```

### 6.3 Command Handler Tests (with InMemory DbContext)

```csharp
public class CreateOrderCommandHandlerTests
{
    private readonly OrderDbContext _dbContext;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly Mock<IOutboxRepository> _outboxMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _productClientMock = new Mock<IProductServiceClient>();
        _outboxMock = new Mock<IOutboxRepository>();
        _handler = new CreateOrderCommandHandler(
            _dbContext,
            _productClientMock.Object,
            _outboxMock.Object);
    }

    [Fact]
    public async Task Handle_WhenStockAvailable_CreatesConfirmedOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            "customer@example.com",
            new[] { new OrderItemDto(Guid.NewGuid(), 2, 49.99m) });

        _productClientMock
            .Setup(x => x.ReserveStockAsync(
                It.IsAny<ReserveStockRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationResult.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(EOrderStatus.Confirmed);

        // Verify order persisted to DbContext
        var savedOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == result.OrderId);
        savedOrder.Should().NotBeNull();
        savedOrder!.Status.Should().Be(EOrderStatus.Confirmed);

        // Verify event queued
        _outboxMock.Verify(
            o => o.AddAsync(It.Is<OrderConfirmedEvent>(e => e.OrderId == result.OrderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStockUnavailable_CreatesRejectedOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            "customer@example.com",
            new[] { new OrderItemDto(Guid.NewGuid(), 2, 49.99m) });

        _productClientMock
            .Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationResult.Failure("Insufficient stock"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(EOrderStatus.Rejected);
        result.Reason.Should().Be("Insufficient stock");

        // Verify order persisted with rejected status
        var savedOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == result.OrderId);
        savedOrder!.Status.Should().Be(EOrderStatus.Rejected);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
```

---

## 7. gRPC Service Testing

### 7.1 TestServerCallContext Helper

For unit testing gRPC services, use a test helper for `ServerCallContext`:

```csharp
// tests/Product.UnitTests/Helpers/TestServerCallContext.cs
public class TestServerCallContext : ServerCallContext
{
    private readonly Metadata _requestHeaders;
    private readonly CancellationToken _cancellationToken;
    private readonly Metadata _responseTrailers = new();
    private readonly Dictionary<object, object> _userState = new();

    private TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken)
    {
        _requestHeaders = requestHeaders ?? new Metadata();
        _cancellationToken = cancellationToken;
    }

    public static TestServerCallContext Create(
        Metadata? requestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        return new TestServerCallContext(requestHeaders ?? new Metadata(), cancellationToken);
    }

    protected override string MethodCore => "TestMethod";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "ipv4:127.0.0.1:12345";
    protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(5);
    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore => _responseTrailers;
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => new(null, new Dictionary<string, List<AuthProperty>>());
    protected override IDictionary<object, object> UserStateCore => _userState;

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        => throw new NotImplementedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        => Task.CompletedTask;
}
```

### 7.2 gRPC Service Test Example

```csharp
public class ProductGrpcServiceTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductGrpcService _service;

    public ProductGrpcServiceTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _service = new ProductGrpcService(_mediatorMock.Object);
    }

    [Fact]
    public async Task ReserveStock_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var context = TestServerCallContext.Create();
        // Note: CorrelationId is propagated via gRPC metadata, not in the request message
        var request = new ReserveStockRequest
        {
            OrderId = Guid.NewGuid().ToString()
        };
        request.Items.Add(new OrderItem { ProductId = Guid.NewGuid().ToString(), Quantity = 2 });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ReserveStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReserveStockResult(true, null, null));

        // Act
        var response = await _service.ReserveStock(request, context);

        // Assert
        response.Success.Should().BeTrue();
        response.FailureReason.Should().BeEmpty();
    }
}
```

---

## 8. Validator Testing

### 8.1 Representative Validator Test

```csharp
public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithEmptyItems_ReturnsError()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), "test@example.com", new List<OrderItemDto>());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public void Validate_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            "test@example.com",
            new[] { new OrderItemDto(Guid.NewGuid(), 1) });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
```

---

## 9. Project Structure

```
tests/
├── Product.UnitTests/
│   ├── Domain/
│   │   └── ProductTests.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   └── ReserveStockCommandHandlerTests.cs
│   │   └── Validators/
│   │       └── CreateProductCommandValidatorTests.cs
│   ├── Grpc/
│   │   └── ProductGrpcServiceTests.cs
│   └── Helpers/
│       └── TestServerCallContext.cs
│
├── Order.UnitTests/
│   ├── Domain/
│   │   ├── OrderTests.cs
│   │   └── OrderStateMachineTests.cs
│   ├── Application/
│   │   └── Commands/
│   │       ├── CreateOrderCommandHandlerTests.cs
│   │       └── CancelOrderCommandHandlerTests.cs
│   └── Helpers/
│       └── TestServerCallContext.cs
│
└── Common.UnitTests/
    └── Domain/
        └── ValueObjectTests.cs
```

---

## Related Documents

- [Functional Testing](./functional-testing.md) - Integration and API testing
- [gRPC Communication](./grpc-communication.md) - TestServerCallContext usage
- [Order Service Interface](./order-service-interface.md) - Domain model details
- [Product Service Interface](./product-service-interface.md) - Domain model details