# Order Service Interface

## Metadata

| Attribute | Value |
|-----------|-------|
| Service Type | ASP.NET Core Web API |
| Port | 5002 (HTTP) |
| Database | PostgreSQL |
| Role | Order lifecycle management, gRPC client |

---

## 1. Overview

Order Service manages the complete order lifecycle. It acts as a **gRPC client** to Product Service for stock operations and **publishes integration events** for notifications.

```
                    ┌─────────────────┐
    HTTP/REST       │                 │      gRPC
   ──────────────▶  │  Order Service  │ ──────────────▶ Product Service
                    │                 │
                    └────────┬────────┘
                             │
                             │ Integration Events
                             ▼
                        ┌──────────┐
                        │ RabbitMQ │
                        └──────────┘
```

---

## 2. HTTP Endpoints

| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| POST | `/orders` | Create new order | 201 Created / 200 OK (Rejected) |
| GET | `/orders/{id}` | Get order details | 200 OK / 404 Not Found |
| GET | `/orders` | List customer orders | 200 OK |
| POST | `/orders/{id}/cancel` | Cancel order | 200 OK / 400 Bad Request |

---

## 3. API Request/Response

### 3.1 Create Order

**Request:**
```json
POST /api/orders
Content-Type: application/json

{
  "customerId": "660e8400-e29b-41d4-a716-446655440001",
  "customerEmail": "customer@example.com",
  "items": [
    {
      "productId": "770e8400-e29b-41d4-a716-446655440002",
      "quantity": 2
    }
  ]
}
```

**Note:** Product name and price are not included in the request. The Order Service fetches product information from Product Service via gRPC `GetProducts` call before creating the order. This ensures:
- Consistent pricing (always uses current product price)
- Validated product existence
- Accurate product names

**Success Response (stock reserved):**
```json
HTTP/1.1 201 Created
Location: /api/orders/550e8400-e29b-41d4-a716-446655440000

{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Confirmed",
  "totalAmount": 299.99,
  "message": "Order confirmed successfully",
  "links": {
    "self": "/api/orders/550e8400-e29b-41d4-a716-446655440000",
    "cancel": "/api/orders/550e8400-e29b-41d4-a716-446655440000/cancel"
  }
}
```

**Rejected Response (insufficient stock):**
```json
HTTP/1.1 200 OK

{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Rejected",
  "reason": "Insufficient stock for product(s): ProductA, ProductB",
  "failedProductIds": [
    "770e8400-e29b-41d4-a716-446655440002",
    "880e8400-e29b-41d4-a716-446655440003"
  ],
  "links": {
    "self": "/api/orders/550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### 3.2 Validation Error

```json
HTTP/1.1 400 Bad Request

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "traceId": "00-84a2f2ad3b5c4e4e8c5a3d4e5f6a7b8c-9d0e1f2a3b4c5d6e-00",
  "errors": {
    "Items": ["At least one item is required"]
  }
}
```

---

## 4. Order Lifecycle

### 4.1 States

| State | Description |
|-------|-------------|
| `Created` | Initial state (transient, during processing) |
| `Confirmed` | Stock reserved successfully |
| `Rejected` | Insufficient stock |
| `Cancelled` | Cancelled by user |
| `Shipped` | Order shipped |

### 4.2 Transitions

```
                    ┌─────────────────┐
                    │     Created     │
                    └────────┬────────┘
                             │
            ┌────────────────┴────────────────┐
            │                                 │
            ▼                                 ▼
   ┌─────────────────┐               ┌─────────────────┐
   │    Confirmed    │               │    Rejected     │
   └────────┬────────┘               └─────────────────┘
            │                                 (final)
     ┌──────┴──────┐
     │             │
     ▼             ▼
┌──────────┐  ┌──────────┐
│ Cancelled│  │ Shipped  │
└──────────┘  └──────────┘
   (final)      (final)
```

### 4.3 Transition Rules

| From | To | Trigger | Action |
|------|-----|---------|--------|
| Created | Confirmed | `ReserveStockResponse.Success = true` | Publish `OrderConfirmed` |
| Created | Rejected | `ReserveStockResponse.Success = false` | Publish `OrderRejected` |
| Confirmed | Cancelled | Cancel request + `ReleaseStockResponse` | Publish `OrderCancelled` |
| Confirmed | Shipped | Ship order | (future) |

---

## 5. Order Flows

### 5.1 Create Order (Happy Path)

```
Step  Service       Action                                    Communication
─────────────────────────────────────────────────────────────────────────────
1     Order        Receive POST /orders                       HTTP/REST
2     Order        Call ProductService.ReserveStock()         gRPC (HTTP/2)
3     Product      Process gRPC request
                   - check availability
                   - reserve stock (create StockReservation)
                   - return ReserveStockResponse (Success)
4     Order        Process response
                   - change state to Confirmed
                   - publish OrderConfirmed                   MassTransit Event
5     Notification Process OrderConfirmed
                   - send email to customer
6     Order        Return HTTP 201 Created                    HTTP/REST
```

### 5.2 Create Order (Failure Path)

```
Step  Service       Action                                    Communication
─────────────────────────────────────────────────────────────────────────────
1     Order        Receive POST /orders                       HTTP/REST
2     Order        Call ProductService.ReserveStock()         gRPC (HTTP/2)
3     Product      Process gRPC request
                   - insufficient stock
                   - return ReserveStockResponse (Failure)
4     Order        Process response
                   - change state to Rejected
                   - publish OrderRejected                    MassTransit Event
5     Notification Process OrderRejected
                   - send email to customer
6     Order        Return HTTP 200 OK (status: Rejected)      HTTP/REST
```

### 5.3 Cancel Order

```
Step  Service       Action                                    Communication
─────────────────────────────────────────────────────────────────────────────
1     Order        Receive POST /orders/{id}/cancel           HTTP/REST
2     Order        Call ProductService.ReleaseStock()         gRPC (HTTP/2)
3     Product      Process gRPC request
                   - find reservation by OrderId
                   - release reservation
                   - return ReleaseStockResponse
4     Order        Process response
                   - change state to Cancelled
                   - publish OrderCancelled                   MassTransit Event
5     Notification Process OrderCancelled
                   - send email to customer
6     Order        Return HTTP 200 OK                         HTTP/REST
```

---

## 6. Domain Model

### 6.1 Order Aggregate

```csharp
public class OrderEntity
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public EOrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<OrderItemEntity> _items = new();
    public IReadOnlyList<OrderItemEntity> Items => _items.AsReadOnly();

    // Factory method
    public static OrderEntity Create(Guid customerId, string customerEmail, IEnumerable<OrderItemEntity> items)
    {
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            Status = EOrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };
        order._items.AddRange(items);
        order.TotalAmount = order._items.Sum(i => i.UnitPrice * i.Quantity);
        return order;
    }

    public void Confirm()
    {
        if (Status != EOrderStatus.Created)
            throw new InvalidOperationException($"Cannot confirm order in {Status} state");

        Status = EOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != EOrderStatus.Created)
            throw new InvalidOperationException($"Cannot reject order in {Status} state");

        Status = EOrderStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status != EOrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot cancel order in {Status} state");

        Status = EOrderStatus.Cancelled;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### 6.2 Order Item

```csharp
public class OrderItemEntity
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
}
```

### 6.3 Order Status

```csharp
public enum EOrderStatus
{
    Created,
    Confirmed,
    Rejected,
    Cancelled,
    Shipped
}
```

---

## 7. Internal API Dependencies

Order Service depends on Product Service for stock operations. Communication uses gRPC via `IProductServiceClient` abstraction. For technical patterns, see [gRPC Communication](./grpc-communication.md).

### 7.1 Product Service Client Interface

```csharp
namespace EShop.Contracts.ServiceClients.Product;

public interface IProductServiceClient
{
    Task<GetProductsResult> GetProductsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default);

    Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default);
}
```

### 7.2 Request/Response Models

```csharp
// === GetProducts ===

public sealed record GetProductsResult(
    IReadOnlyList<ProductInfo> Products);

public sealed record ProductInfo(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);

// === ReserveStock ===

public sealed record ReserveStockRequest(
    Guid OrderId,
    IReadOnlyList<OrderItemRequest> Items);

public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);

public sealed record StockReservationResult(
    bool Success,
    EStockReservationErrorCode? ErrorCode = null,
    string? FailureReason = null,
    IReadOnlyList<Guid>? FailedProductIds = null);

// === ReleaseStock ===

public sealed record ReleaseStockRequest(
    Guid OrderId);

public sealed record StockReleaseResult(
    bool Success,
    string? FailureReason = null);
```

### 7.3 Client Configuration

```json
// Order.API/appsettings.json
{
  "ServiceClients": {
    "ProductService": {
      "Url": "https://localhost:5051"
    }
  }
}
```

### 7.4 DI Registration

```csharp
// Order.API/Program.cs
builder.Services.AddServiceClients(builder.Configuration);
```

### 7.5 Usage in Handlers

```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderDbContext _db;
    private readonly IProductServiceClient _productClient;
    private readonly IDateTimeProvider _dateTimeProvider;

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // 1. Get product info (name, price) from Product Service
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var productsResult = await _productClient.GetProductsAsync(productIds, ct);
        var productLookup = productsResult.Products.ToDictionary(p => p.ProductId);

        // 2. Create order items with product info
        var orderItems = request.Items.Select(i =>
        {
            var product = productLookup[i.ProductId];
            return OrderItem.Create(i.ProductId, product.Name, i.Quantity, product.Price);
        });

        // 3. Create order
        var order = OrderEntity.Create(
            request.CustomerId,
            request.CustomerEmail,
            orderItems,
            _dateTimeProvider.UtcNow);

        // 4. Reserve stock via Internal API
        var stockItems = request.Items
            .Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
            .ToList();
        var reserveResult = await _productClient.ReserveStockAsync(
            new ReserveStockRequest(order.Id, stockItems), ct);

        // 5. Handle result
        if (reserveResult.Success)
        {
            order.Confirm(_dateTimeProvider.UtcNow);
            // ... publish OrderConfirmedEvent
        }
        else
        {
            order.Reject(reserveResult.FailureReason!, _dateTimeProvider.UtcNow);
            // ... publish OrderRejectedEvent
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return new CreateOrderResult(order.Id, order.Status.ToString(), reserveResult.FailureReason);
    }
}
```

### 7.6 Cancel Order Handler

```csharp
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderDbContext _db;
    private readonly IProductServiceClient _productClient;
    private readonly IOutboxRepository _outbox;

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync(request.OrderId, ct)
            ?? throw new NotFoundException($"Order {request.OrderId} not found");

        // Release stock via Internal API
        var releaseResult = await _productClient.ReleaseStockAsync(
            new ReleaseStockRequest(order.Id),
            ct);

        if (releaseResult.Success)
        {
            order.Cancel(request.Reason);
            await _outbox.AddAsync(new OrderCancelledEvent(
                order.Id, order.CustomerId, request.Reason), ct);
        }
        else
        {
            // Log failure but don't fail the cancellation
            _logger.LogWarning(
                "Failed to release stock for order {OrderId}: {Reason}",
                order.Id, releaseResult.FailureReason);
        }

        await _db.SaveChangesAsync(ct);
        return new CancelOrderResult(order.Id, order.Status);
    }
}
```

---

## 8. Published Events

| Event | Trigger | Data |
|-------|---------|------|
| `OrderConfirmed` | Stock reserved | OrderId, CustomerId, CustomerEmail, TotalAmount, Items |
| `OrderRejected` | Stock unavailable | OrderId, CustomerId, Reason |
| `OrderCancelled` | User cancellation | OrderId, CustomerId, Reason |

> **Note**: `OrderConfirmed` includes `CustomerEmail` and `Items` as a pragmatic exception to the thin-events principle. See [Event Design Philosophy](./messaging-communication.md#3-event-design-philosophy) for rationale.

See [Messaging Communication](./messaging-communication.md) for event definitions.

---

## 9. Project Structure

```
Order.API/
├── Controllers/
│   └── OrdersController.cs
└── Program.cs

Order.Application/
├── Commands/
│   ├── CreateOrder/
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateOrderCommandHandler.cs
│   │   └── CreateOrderCommandValidator.cs
│   └── CancelOrder/
├── Queries/
│   ├── GetOrders/
│   └── GetOrderById/
├── Data/
│   └── IOrderDbContext.cs           # DbContext interface (NO Repository)
└── Behaviors/                       # (from EShop.Common)

Order.Domain/
├── Entities/
│   ├── OrderEntity.cs               # Inherits from AggregateRoot (EShop.SharedKernel)
│   └── OrderItemEntity.cs
├── Enums/
│   └── EEOrderStatus.cs              # Following naming convention
├── StateMachine/
│   └── OrderStateMachine.cs
└── Exceptions/
    └── InvalidOrderStateException.cs

Order.Infrastructure/
├── Data/
│   ├── OrderDbContext.cs            # Implements IOrderDbContext
│   └── Configurations/
│       ├── OrderConfiguration.cs
│       └── OrderItemConfiguration.cs
└── Outbox/                          # Uses base from EShop.Common
    └── OutboxProcessor.cs
```

### 9.1 DbContext Interface Pattern

```csharp
// Order.Application/Data/IOrderDbContext.cs
public interface IOrderDbContext
{
    DbSet<OrderEntity> Orders { get; }
    DbSet<OrderItemEntity> OrderItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Order.Infrastructure/Data/OrderDbContext.cs
public class OrderDbContext : DbContext, IOrderDbContext
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    // EF Core configuration...
}
```

### 9.2 Handler Example (using DbContext)

See Section 7.5 for the complete handler implementation with `GetProductsAsync` call.

**Note**: `IProductServiceClient` comes from `EShop.ServiceClients` - it's the gRPC client abstraction for internal service communication.

---

## 10. Internal gRPC API

Order Service exposes an **Internal gRPC API** for service-to-service communication. This is used by Notification Service to fetch order details (including customer email) when processing integration events.

### 10.1 Service Definition

```protobuf
// order.proto
syntax = "proto3";

option csharp_namespace = "EShop.Grpc.Order";

package order;

service OrderService {
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
  string total_amount = 5;
  repeated OrderItemInfo items = 6;
}

message OrderItemInfo {
  string product_id = 1;
  string product_name = 2;
  int32 quantity = 3;
  string unit_price = 4;
}
```

### 10.2 Use Case: Notification Service

When Notification Service receives an `OrderConfirmed` event, it needs the customer email to send a confirmation email. The event intentionally does NOT contain the email (loose coupling). Instead:

```
1. Notification Service receives OrderConfirmed event
2. Calls Order Service: GetOrderDetails(orderId)
3. Receives response with CustomerEmail
4. Sends confirmation email
```

### 10.3 Server Implementation

```csharp
public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderGrpcService> _logger;

    public override async Task<GetOrderDetailsResponse> GetOrderDetails(
        GetOrderDetailsRequest request, ServerCallContext context)
    {
        _logger.LogDebug("GetOrderDetails called for order {OrderId}", request.OrderId);

        var query = new GetOrderByIdQuery(Guid.Parse(request.OrderId));
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

### 10.4 Registration

```csharp
// Order.API/Program.cs
app.MapGrpcService<OrderGrpcService>();
```

See [Order Internal API](./order-internal-api.md) for complete specification.

---

## Related Documents

- [Internal API Communication](./internal-api-communication.md) - Internal API layer concept
- [gRPC Communication](./grpc-communication.md) - gRPC technical patterns
- [Product Service Interface](./product-service-interface.md) - Product Service contracts (Internal API server)
- [Messaging Communication](./messaging-communication.md) - Event publishing
- [CorrelationId Flow](./correlation-id-flow.md) - Request tracing
