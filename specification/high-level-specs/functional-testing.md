# Functional Testing Architecture

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Integration and API testing |
| Applies To | All services |
| Framework | xUnit, WebApplicationFactory, Testcontainers, Respawn, WireMock |

---

## 1. Overview

Functional testing verifies the system works correctly with real infrastructure components. The strategy prioritizes **fast execution** and **simplicity** over exhaustive E2E orchestration.

---

## 2. Testing Stack

| Tool | Purpose |
|------|---------|
| **WebApplicationFactory** | In-memory API endpoint testing |
| **Testcontainers** | Real PostgreSQL and RabbitMQ containers |
| **Respawn** | Fast database reset between tests |
| **WireMock** | Mock external HTTP APIs (SendGrid) |
| **FluentAssertions** | Readable assertions |

---

## 3. Testing Strategy

### 3.1 Fast Execution Principles

| Principle | Implementation |
|-----------|----------------|
| Minimize network overhead | Use WebApplicationFactory for most API tests |
| Share containers | Spin up Testcontainers once per test run |
| Fast cleanup | Use Respawn instead of recreating containers |
| Parallel execution | Parallelize tests where possible |

### 3.2 Simplicity Principles

| Principle | Implementation |
|-----------|----------------|
| No E2E orchestration | Skip Docker Compose for tests |
| Minimal fixtures | Reuse shared test infrastructure |
| Clear separation | Unit tests (Moq) vs Functional tests (real infra) |

---

## 4. Tool Responsibilities

### 4.1 WebApplicationFactory

Tests API layer without network overhead:

- API endpoint routing and model binding
- Request/response serialization
- Authentication/authorization middleware
- Validation behavior
- MediatR pipeline integration

### 4.2 Testcontainers + PostgreSQL

Tests data layer with real database:

- Repository implementations
- EF Core queries and LINQ translations
- Migration compatibility
- Transaction behavior
- Concurrent access patterns

### 4.3 Testcontainers + RabbitMQ

Tests messaging layer:

- MassTransit consumer/publisher integration
- Message serialization/deserialization
- Outbox pattern (publish within transaction)
- Inbox pattern (idempotent consumption)
- Dead letter queue behavior

### 4.4 WireMock

Tests external API integrations:

- Notification Service → SendGrid integration
- Error handling for external API failures
- Retry/circuit breaker behavior
- Timeout handling

---

## 5. WebApplicationFactory Setup

### 5.1 Custom Factory Base

```csharp
// tests/Product.IntegrationTests/Infrastructure/ProductApiFactory.cs
public class ProductApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("products_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace real database with Testcontainer
            services.RemoveAll<DbContextOptions<ProductDbContext>>();
            services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Replace RabbitMQ with in-memory transport for isolated tests
            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Apply migrations
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### 5.2 Respawn for Fast Cleanup

```csharp
// tests/Product.IntegrationTests/Infrastructure/DatabaseFixture.cs
public class DatabaseFixture : IAsyncLifetime
{
    private Respawner _respawner = null!;
    private string _connectionString = null!;

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        _respawner = await Respawner.CreateAsync(_connectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connectionString);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

---

## 6. API Endpoint Testing

### 6.1 Product API Tests

```csharp
public class ProductsControllerTests : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient _client;
    private readonly ProductApiFactory _factory;

    public ProductsControllerTests(ProductApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 99.99m,
            StockQuantity = 100,
            LowStockThreshold = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task CreateProduct_WithMissingName_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Price = 99.99m,
            StockQuantity = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task GetProduct_WhenExists_Returns200Ok()
    {
        // Arrange - Create a product first
        var createRequest = new CreateProductRequest { Name = "Test", Price = 10m, StockQuantity = 50 };
        var createResponse = await _client.PostAsJsonAsync("/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        // Act
        var response = await _client.GetAsync($"/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetProduct_WhenNotExists_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

### 6.2 Order API Tests

```csharp
public class OrdersControllerTests : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(OrderApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_WithAvailableStock_Returns201Confirmed()
    {
        // Arrange - Setup mock product service to return success
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Items = new[] { new OrderItemRequest(Guid.NewGuid(), 2) }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelOrder_WhenConfirmed_Returns200Ok()
    {
        // Arrange - Create and confirm an order first
        // ...

        // Act
        var response = await _client.PostAsync($"/orders/{orderId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Status.Should().Be("Cancelled");
    }
}
```

---

## 7. Database Integration Testing

### 7.1 DbContext Tests with Testcontainers

```csharp
public class ProductDbContextTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IProductDbContext _context = null!;

    public ProductDbContextTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _context = new ProductDbContext(/* connection string */);
    }

    [Fact]
    public async Task Add_WithValidProduct_PersistsToDatabase()
    {
        // Arrange
        var product = new Product("Test Product", 99.99m, 100, 10);

        // Act
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Assert
        var persisted = await _context.Products.FindAsync(product.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task Find_WhenExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product("Test", 10m, 50, 5);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act - Direct DbContext access via interface
        var result = await _context.Products.FindAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    public Task DisposeAsync()
    {
        ((IDisposable)_context).Dispose();
        return Task.CompletedTask;
    }
}
```

---

## 8. Messaging Integration Testing

### 8.1 MassTransit Test Harness

```csharp
public class OrderConfirmedConsumerTests : IClassFixture<NotificationWorkerFactory>
{
    private readonly ITestHarness _harness;

    public OrderConfirmedConsumerTests(NotificationWorkerFactory factory)
    {
        _harness = factory.Services.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Consume_WithValidEvent_SendsEmail()
    {
        // Arrange
        await _harness.Start();

        var @event = new OrderConfirmed
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            TotalAmount = 199.99m,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<OrderConfirmed>()).Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<OrderConfirmedConsumer>();
        (await consumerHarness.Consumed.Any<OrderConfirmed>()).Should().BeTrue();
    }
}
```

### 8.2 Outbox Pattern Testing

```csharp
public class OutboxIntegrationTests : IClassFixture<ProductApiFactory>
{
    [Fact]
    public async Task CreateProduct_PublishesEventViaOutbox()
    {
        // Arrange
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act - Create product via API
        var response = await _client.PostAsJsonAsync("/products", new CreateProductRequest { /* ... */ });

        // Assert - Verify event was published via outbox
        await Task.Delay(1000); // Wait for outbox processor
        (await harness.Published.Any<ProductCreated>()).Should().BeTrue();
    }
}
```

---

## 9. External API Testing with WireMock

### 9.1 SendGrid Mock Setup

```csharp
public class NotificationWorkerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private WireMockServer _wireMock = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace SendGrid base URL with WireMock
            services.Configure<EmailOptions>(options =>
            {
                options.BaseUrl = _wireMock.Url!;
                options.ApiKey = "test-api-key";
            });
        });
    }

    public Task InitializeAsync()
    {
        _wireMock = WireMockServer.Start();

        // Setup default SendGrid success response
        _wireMock.Given(
            Request.Create()
                .WithPath("/mail/send")
                .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(202));

        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        _wireMock.Stop();
        return Task.CompletedTask;
    }
}
```

### 9.2 Testing External API Failures

```csharp
public class EmailServiceResilienceTests : IClassFixture<NotificationWorkerFactory>
{
    private readonly WireMockServer _wireMock;

    [Fact]
    public async Task SendEmail_WhenSendGridReturns500_RetriesAndFails()
    {
        // Arrange - Setup WireMock to return 500
        _wireMock.Reset();
        _wireMock.Given(Request.Create().WithPath("/mail/send").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        // Act
        var result = await _emailService.SendAsync(new EmailMessage(/* ... */));

        // Assert
        result.Success.Should().BeFalse();

        // Verify retry attempts
        _wireMock.LogEntries.Count.Should().Be(4); // 1 + 3 retries
    }

    [Fact]
    public async Task SendEmail_WhenSendGridReturns429_RespectsRateLimit()
    {
        // Arrange
        _wireMock.Reset();
        _wireMock.Given(Request.Create().WithPath("/mail/send").UsingPost())
            .InScenario("RateLimit")
            .WillSetStateTo("First")
            .RespondWith(Response.Create().WithStatusCode(429));

        _wireMock.Given(Request.Create().WithPath("/mail/send").UsingPost())
            .InScenario("RateLimit")
            .WhenStateIs("First")
            .RespondWith(Response.Create().WithStatusCode(202));

        // Act
        var result = await _emailService.SendAsync(new EmailMessage(/* ... */));

        // Assert
        result.Success.Should().BeTrue();
    }
}
```

---

## 10. Project Structure

```
tests/
├── Product.IntegrationTests/
│   ├── Infrastructure/
│   │   ├── ProductApiFactory.cs
│   │   └── DatabaseFixture.cs
│   ├── Controllers/
│   │   └── ProductsControllerTests.cs
│   ├── Repositories/
│   │   └── ProductRepositoryTests.cs
│   └── Grpc/
│       └── ProductGrpcServiceIntegrationTests.cs
│
├── Order.IntegrationTests/
│   ├── Infrastructure/
│   │   └── OrderApiFactory.cs
│   ├── Controllers/
│   │   └── OrdersControllerTests.cs
│   └── Messaging/
│       └── OutboxIntegrationTests.cs
│
├── Notification.IntegrationTests/
│   ├── Infrastructure/
│   │   └── NotificationWorkerFactory.cs
│   ├── Consumers/
│   │   └── OrderConfirmedConsumerTests.cs
│   └── Services/
│       └── EmailServiceResilienceTests.cs
│
└── Architecture.Tests/
    └── LayerDependencyTests.cs
```

---

## Related Documents

- [Unit Testing](./unit-testing.md) - Unit testing strategy
- [gRPC Communication](./grpc-communication.md) - gRPC testing patterns
- [Messaging Communication](./messaging-communication.md) - Message consumer testing