# Order Internal API (gRPC)

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Internal API - service-to-service communication |
| Services | Notification Service (client) ↔ Order Service (server) |
| Protocol | HTTP/2, Protocol Buffers |
| Package | `EShop.Grpc` |
| API Layer | **Internal API** (not exposed via API Gateway) |
| Related | [Order Service Interface](./order-service-interface.md) |

---

## 1. Overview

Order Service exposes an Internal gRPC API for other services to fetch order details. This enables **loose coupling** in the messaging architecture - integration events contain only identifiers, and consumers fetch additional data when needed.

```
                         INTERNAL API (gRPC)
┌─────────────────┐                                ┌─────────────┐
│ Notification    │ ──────────────────────────────▶│ Order       │
│ Service         │◀────────────────────────────── │ Service     │
│ (Client)        │   GetOrderDetails              │ (Server)    │
└─────────────────┘                                └─────────────┘
```

### 1.1 Why This API Exists

Integration events follow the [Event Design Philosophy](./messaging-communication.md#3-event-design-philosophy):

- Events contain **identifiers** (OrderId, CustomerId) and **immutable facts** (TotalAmount)
- `CustomerEmail` is included as a **pragmatic exception** (see [Event Design Philosophy](./messaging-communication.md#3-event-design-philosophy))
- The Internal API exists for consumers who need **additional order details** beyond what's in the event (e.g., full item list with product names, shipping address)

**Use Cases for Internal API:**
| Scenario | Solution |
|----------|----------|
| Send order confirmation email | Use `CustomerEmail` from event (pragmatic exception) |
| Display order details in admin panel | Fetch via Internal API |
| Generate invoice with full details | Fetch via Internal API |

---

## 2. Proto Definition

**File:** `src/Common/EShop.Grpc/Protos/order.proto`

```protobuf
syntax = "proto3";

option csharp_namespace = "EShop.Grpc.Order";

package order;

// Internal API - for service-to-service communication
// Primary consumer: Notification Service
service OrderService {
  // Get order details including customer email
  rpc GetOrderDetails (GetOrderDetailsRequest) returns (GetOrderDetailsResponse);
}

message GetOrderDetailsRequest {
  string order_id = 1;
}

message GetOrderDetailsResponse {
  string order_id = 1;
  string customer_id = 2;
  string customer_email = 3;
  string status = 4;
  string total_amount = 5;  // decimal as string for precision
  repeated OrderItemInfo items = 6;
}

message OrderItemInfo {
  string product_id = 1;
  string product_name = 2;
  int32 quantity = 3;
  string unit_price = 4;    // decimal as string for precision
}
```

### 2.1 Design Decisions

| Decision | Rationale |
|----------|-----------|
| `total_amount` as string | Protocol Buffers lacks native decimal; string preserves precision |
| `unit_price` as string | Same reason - monetary precision |
| No `correlation_id` in messages | Propagated via gRPC metadata (interceptors), not payload |
| Include `items` in response | Allows consumer to build detailed email without additional calls |

---

## 3. Service Contract

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| `GetOrderDetails` | `GetOrderDetailsRequest` | `GetOrderDetailsResponse` | Get full order details including customer email |

### 3.1 Error Responses

| Scenario | gRPC Status Code | Description |
|----------|------------------|-------------|
| Order not found | `NotFound` | Order with given ID doesn't exist |
| Invalid order ID format | `InvalidArgument` | Order ID is not a valid GUID |
| Internal error | `Internal` | Unexpected server error |

---

## 4. Server Implementation (Order Service)

**File:** `src/Services/Order/Order.API/Grpc/OrderGrpcService.cs`

```csharp
public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(IMediator mediator, ILogger<OrderGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetOrderDetailsResponse> GetOrderDetails(
        GetOrderDetailsRequest request, ServerCallContext context)
    {
        _logger.LogDebug("GetOrderDetails called for order {OrderId}", request.OrderId);

        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Invalid order ID format: {request.OrderId}"));
        }

        var query = new GetOrderByIdQuery(orderId);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (result is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Order {request.OrderId} not found"));
        }

        var response = new GetOrderDetailsResponse
        {
            OrderId = result.Id.ToString(),
            CustomerId = result.CustomerId.ToString(),
            CustomerEmail = result.CustomerEmail,
            Status = result.Status.ToString(),
            TotalAmount = result.TotalAmount.ToString(CultureInfo.InvariantCulture)
        };

        response.Items.AddRange(result.Items.Select(i => new OrderItemInfo
        {
            ProductId = i.ProductId.ToString(),
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice.ToString(CultureInfo.InvariantCulture)
        }));

        return response;
    }
}
```

### 4.1 Registration

```csharp
// Order.API/Program.cs
builder.Services.AddGrpc(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});

// ...

app.MapGrpcService<OrderGrpcService>();
```

---

## 5. Client Implementation (Notification Service)

### 5.1 Client Interface

**File:** `src/Common/EShop.ServiceClients/IOrderServiceClient.cs`

```csharp
public interface IOrderServiceClient
{
    Task<OrderDetailsResult> GetOrderDetailsAsync(Guid orderId, CancellationToken ct);
}

public record OrderDetailsResult(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemInfo> Items);

public record OrderItemInfo(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
```

### 5.2 gRPC Client Implementation

**File:** `src/Common/EShop.ServiceClients/Grpc/OrderGrpcClient.cs`

```csharp
public class OrderGrpcClient : IOrderServiceClient
{
    private readonly OrderService.OrderServiceClient _client;
    private readonly ILogger<OrderGrpcClient> _logger;

    public OrderGrpcClient(
        OrderService.OrderServiceClient client,
        ILogger<OrderGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OrderDetailsResult> GetOrderDetailsAsync(Guid orderId, CancellationToken ct)
    {
        _logger.LogDebug("Getting order details for {OrderId}", orderId);

        var request = new GetOrderDetailsRequest
        {
            OrderId = orderId.ToString()
        };

        var response = await _client.GetOrderDetailsAsync(
            request,
            deadline: DateTime.UtcNow.AddSeconds(30),
            cancellationToken: ct);

        return new OrderDetailsResult(
            Guid.Parse(response.OrderId),
            Guid.Parse(response.CustomerId),
            response.CustomerEmail,
            response.Status,
            decimal.Parse(response.TotalAmount, CultureInfo.InvariantCulture),
            response.Items.Select(i => new OrderItemInfo(
                Guid.Parse(i.ProductId),
                i.ProductName,
                i.Quantity,
                decimal.Parse(i.UnitPrice, CultureInfo.InvariantCulture))).ToList());
    }
}
```

### 5.3 HTTP Client Implementation (Alternative)

**File:** `src/Common/EShop.ServiceClients/Http/OrderHttpClient.cs`

```csharp
public class OrderHttpClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderHttpClient> _logger;

    public OrderHttpClient(HttpClient httpClient, ILogger<OrderHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OrderDetailsResult> GetOrderDetailsAsync(Guid orderId, CancellationToken ct)
    {
        _logger.LogDebug("Getting order details via HTTP for {OrderId}", orderId);

        var response = await _httpClient.GetFromJsonAsync<OrderDetailsDto>(
            $"/api/orders/{orderId}",
            cancellationToken: ct);

        if (response is null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        return new OrderDetailsResult(
            response.OrderId,
            response.CustomerId,
            response.CustomerEmail,
            response.Status,
            response.TotalAmount,
            response.Items.Select(i => new OrderItemInfo(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice)).ToList());
    }
}
```

### 5.4 DI Registration

```csharp
// Notification.API/Program.cs

// Option 1: gRPC (default)
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:OrderService"]!);
});
builder.Services.AddScoped<IOrderServiceClient, OrderGrpcClient>();

// Option 2: HTTP (configurable)
// builder.Services.AddHttpClient<IOrderServiceClient, OrderHttpClient>(c =>
// {
//     c.BaseAddress = new Uri(builder.Configuration["HttpServices:OrderService"]!);
// });
```

---

## 6. Usage in Notification Service

### 6.1 OrderConfirmedConsumer

```csharp
public class OrderConfirmedConsumer : IdempotentConsumer<OrderConfirmed>
{
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(
        IOrderServiceClient orderServiceClient,
        IEmailService emailService,
        ILogger<OrderConfirmedConsumer> logger,
        InboxDbContext inboxDbContext) : base(inboxDbContext, logger)
    {
        _orderServiceClient = orderServiceClient;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ProcessMessage(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;

        // Fetch customer email via Internal API
        var orderDetails = await _orderServiceClient.GetOrderDetailsAsync(
            message.OrderId,
            context.CancellationToken);

        // Build and send confirmation email
        var result = await _emailService.SendAsync(new EmailMessage(
            To: orderDetails.CustomerEmail,
            Subject: $"Order #{message.OrderId} confirmed",
            HtmlBody: BuildConfirmationEmailBody(message, orderDetails)
        ));

        if (!result.Success)
        {
            _logger.LogWarning(
                "Failed to send order confirmation email for {OrderId}: {Error}",
                message.OrderId, result.ErrorMessage);
        }
    }

    private static string BuildConfirmationEmailBody(
        OrderConfirmed message, OrderDetailsResult orderDetails)
    {
        var itemsList = string.Join("", orderDetails.Items.Select(i =>
            $"<li>{i.ProductName} x {i.Quantity} - {i.UnitPrice:N2} CZK</li>"));

        return $"""
            <h1>Thank you for your order!</h1>
            <p>Your order <strong>#{message.OrderId}</strong> has been confirmed.</p>

            <h2>Order Items</h2>
            <ul>{itemsList}</ul>

            <p><strong>Total: {message.TotalAmount:N2} CZK</strong></p>

            <p>We will notify you when your order ships.</p>
            """;
    }
}
```

---

## 7. Resiliency

### 7.1 gRPC Deadlines

Every call includes a deadline to prevent infinite waiting:

```csharp
var response = await _client.GetOrderDetailsAsync(
    request,
    deadline: DateTime.UtcNow.AddSeconds(30),
    cancellationToken: ct);
```

### 7.2 Retry Policy (Polly)

```csharp
builder.Services
    .AddGrpcClient<OrderService.OrderServiceClient>(o =>
    {
        o.Address = new Uri("http://order-service:5052");
    })
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
}
```

### 7.3 Failure Scenarios

| Scenario | Behavior | Recovery |
|----------|----------|----------|
| Order Service timeout | Retry 3x, then fail | Consumer throws, MassTransit retries message |
| Order Service down | Immediate fail | Consumer throws, MassTransit retries message |
| Order not found | `RpcException(NotFound)` | Log error, don't retry (permanent failure) |

> **Note**: If Order Service is unavailable, the consumer throws and MassTransit retry policy handles it. After exhausted retries, message goes to dead letter queue.

---

## 8. Testing

### 8.1 Unit Testing gRPC Service

```csharp
public class OrderGrpcServiceTests
{
    [Fact]
    public async Task GetOrderDetails_WithValidOrderId_ReturnsOrderDetails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderDto
            {
                Id = orderId,
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "test@example.com",
                Status = EOrderStatus.Confirmed,
                TotalAmount = 100m,
                Items = new List<OrderItemDto>()
            });

        var service = new OrderGrpcService(mediatorMock.Object, Mock.Of<ILogger<OrderGrpcService>>());
        var context = TestServerCallContext.Create();

        // Act
        var response = await service.GetOrderDetails(
            new GetOrderDetailsRequest { OrderId = orderId.ToString() },
            context);

        // Assert
        response.OrderId.Should().Be(orderId.ToString());
        response.CustomerEmail.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetOrderDetails_WithInvalidOrderId_ThrowsInvalidArgument()
    {
        // Arrange
        var service = new OrderGrpcService(Mock.Of<IMediator>(), Mock.Of<ILogger<OrderGrpcService>>());
        var context = TestServerCallContext.Create();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetOrderDetails(
                new GetOrderDetailsRequest { OrderId = "not-a-guid" },
                context));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }
}
```

### 8.2 Integration Testing Consumer

```csharp
public class OrderConfirmedConsumerTests : IClassFixture<NotificationWorkerFactory>
{
    [Fact]
    public async Task Consume_WithValidEvent_CallsOrderServiceAndSendsEmail()
    {
        // Arrange
        var orderServiceMock = new Mock<IOrderServiceClient>();
        orderServiceMock
            .Setup(x => x.GetOrderDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderDetailsResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "customer@example.com",
                "Confirmed",
                199.99m,
                new List<OrderItemInfo>()));

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailResult(Success: true));

        // ... test consumer with mocks
    }
}
```

---

## Related Documents

- [Order Service Interface](./order-service-interface.md) - Order Service HTTP API and domain model
- [gRPC Communication](./grpc-communication.md) - gRPC patterns for Product Service
- [Messaging Communication](./messaging-communication.md) - Integration events and Outbox/Inbox patterns
- 
- [CorrelationId Flow](./correlation-id-flow.md) - Request tracing across gRPC calls
