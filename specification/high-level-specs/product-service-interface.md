# Product Service Interface

## Metadata

| Attribute | Value |
|-----------|-------|
| Service Type | ASP.NET Core Web API + gRPC |
| Ports | 5001 (HTTP), 5051 (gRPC) |
| Database | PostgreSQL |
| Role | Product catalog, stock management |
| API Layers | **External API** (REST) + **Internal API** (gRPC) |

---

## 1. Overview

Product Service manages the product catalog and stock operations. It exposes two API layers:

| Layer | Protocol | Purpose | Consumers |
|-------|----------|---------|-----------|
| **External API** | REST `/api/*` | Product CRUD operations | Clients via API Gateway |
| **Internal API** | gRPC | Stock operations, batch queries | Other microservices |

```
                        ┌─────────────────────────────────┐
                        │        Product Service          │
                        │                                 │
  EXTERNAL API          │  ┌───────────────────────────┐  │
  (via API Gateway)     │  │ /api/products             │  │
  ─────────────────────▶│  │ (REST - CRUD operations)  │  │
                        │  └───────────────────────────┘  │
                        │                                 │
  INTERNAL API          │  ┌───────────────────────────┐  │
  (direct, no Gateway)  │  │ gRPC: GetProducts,        │  │
  ─────────────────────▶│  │       ReserveStock,       │  │
                        │  │       ReleaseStock        │  │
                        │  └───────────────────────────┘  │
                        │                                 │
                        └──────────────┬──────────────────┘
                                       │
                                       │ Integration Events
                                       ▼
                                  ┌──────────┐
                                  │ RabbitMQ │
                                  └──────────┘
```

---

## 2. External API (REST - `/api/*`)

Client-facing endpoints routed via API Gateway.

| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/api/products` | List products (with filtering & pagination) | 200 OK |
| GET | `/api/products/{id}` | Get product details | 200 OK / 404 Not Found |
| POST | `/api/products` | Create product | 201 Created |
| PUT | `/api/products/{id}` | Update product | 200 OK / 404 Not Found |

### 2.1 List Products

**Request:**
```
GET /api/products?category=electronics&page=1&pageSize=20
```

**Response:**
```json
HTTP/1.1 200 OK

{
  "items": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440002",
      "name": "Wireless Mouse",
      "description": "Ergonomic wireless mouse",
      "price": 49.99,
      "stockQuantity": 150,
      "category": "electronics"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

### 2.2 Create Product

**Request:**
```json
POST /api/products
Content-Type: application/json

{
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse",
  "price": 49.99,
  "stockQuantity": 150,
  "lowStockThreshold": 20,
  "category": "electronics"
}
```

**Response:**
```json
HTTP/1.1 201 Created
Location: /api/products/770e8400-e29b-41d4-a716-446655440002

{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse",
  "price": 49.99,
  "stockQuantity": 150,
  "lowStockThreshold": 20,
  "category": "electronics",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## 3. Internal API Contracts

Internal API for service-to-service communication using gRPC. For technical implementation patterns, see [gRPC Communication](./grpc-communication.md).

### 3.1 gRPC Service Definition (`product.proto`)

```protobuf
// Product Service - Internal gRPC API for inter-service communication.

syntax = "proto3";

option csharp_namespace = "EShop.Grpc.Product";

package product;

service ProductService {
  // Batch get product info. ATOMIC: fails with NOT_FOUND if any product missing.
  rpc GetProducts(GetProductsRequest) returns (GetProductsResponse);

  // Reserve stock for order. All-or-nothing: fails if any item has insufficient stock.
  rpc ReserveStock(ReserveStockRequest) returns (ReserveStockResponse);

  // Release stock reservation. Idempotent: succeeds even if already released.
  rpc ReleaseStock(ReleaseStockRequest) returns (ReleaseStockResponse);
}

message GetProductsRequest {
  repeated string product_ids = 1;
}

message GetProductsResponse {
  repeated ProductInfo products = 1;
}

message ProductInfo {
  string product_id = 1;
  string name = 2;
  string description = 3;
  string price = 4;  // decimal as string
  int32 stock_quantity = 5;
}

message ReserveStockRequest {
  string order_id = 1;
  repeated OrderItem items = 2;
}

message OrderItem {
  string product_id = 1;
  int32 quantity = 2;
}

message ReserveStockResponse {
  bool success = 1;
  optional string failure_reason = 2;
}

message ReleaseStockRequest {
  string order_id = 1;
}

message ReleaseStockResponse {
  bool success = 1;
  optional string failure_reason = 2;
}
```

### 3.2 Proto Design Decisions

| Decision | Rationale |
|----------|-----------|
| `GetProducts` is **ATOMIC** | Per [Google AIP-231](https://google.aip.dev/231): batch get must succeed for all or fail for all |
| No `exists`/`is_available` fields | Use gRPC `NOT_FOUND` status instead (cleaner API) |
| `price` as string | Protocol Buffers lacks native decimal; string preserves precision |
| All-or-nothing for `ReserveStock` | If any item fails, entire reservation fails |
| No `correlation_id` in messages | Propagated via gRPC metadata (interceptors), not payload |

### 3.3 gRPC Endpoints Summary

| Method | Description | Error Behavior |
|--------|-------------|----------------|
| `GetProducts` | Batch get product info | `NOT_FOUND` if any product missing |
| `ReserveStock` | Reserve stock for order | `success=false` if insufficient stock |
| `ReleaseStock` | Release reservation | Idempotent, always succeeds |

### 3.4 ServiceClients Models

These models are used by consuming services via `IProductServiceClient`:

```csharp
namespace EShop.ServiceClients.Abstractions;

// === GetProducts ===
// Note: Throws NotFoundException if any product is missing (atomic operation)

public sealed record GetProductsRequest(
    IReadOnlyList<Guid> ProductIds);

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
    IReadOnlyList<OrderItemDto> Items);

public sealed record StockReservationResult(
    bool Success,
    string? FailureReason = null);

// === ReleaseStock ===

public sealed record ReleaseStockRequest(
    Guid OrderId);

public sealed record StockReleaseResult(
    bool Success,
    string? FailureReason = null);

// === Shared ===

public sealed record OrderItemDto(
    Guid ProductId,
    int Quantity);
```

### 3.5 Server Implementation

```csharp
public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(IMediator mediator, ILogger<ProductGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetProductsResponse> GetProducts(
        GetProductsRequest request, ServerCallContext context)
    {
        _logger.LogDebug("GetProducts called for {Count} product IDs", request.ProductIds.Count);

        var requestedIds = request.ProductIds.Select(Guid.Parse).ToList();
        var query = new GetProductsBatchQuery(requestedIds);
        var result = await _mediator.Send(query, context.CancellationToken);

        // ATOMIC: fail if any product not found (per Google AIP-231)
        var foundIds = result.Products.Select(p => p.ProductId).ToHashSet();
        var missingIds = requestedIds.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Products not found: {string.Join(", ", missingIds)}"));
        }

        var response = new GetProductsResponse();
        response.Products.AddRange(result.Products.Select(p => new ProductInfo
        {
            ProductId = p.ProductId.ToString(),
            Name = p.Name,
            Description = p.Description,
            Price = p.Price.ToString(CultureInfo.InvariantCulture),
            StockQuantity = p.StockQuantity
        }));

        return response;
    }

    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request, ServerCallContext context)
    {
        _logger.LogDebug("ReserveStock called for OrderId: {OrderId}", request.OrderId);

        var command = new ReserveStockCommand(
            Guid.Parse(request.OrderId),
            request.Items.Select(i => new OrderItemDto(
                Guid.Parse(i.ProductId),
                i.Quantity)).ToList());

        var result = await _mediator.Send(command, context.CancellationToken);

        return new ReserveStockResponse
        {
            Success = result.Success,
            FailureReason = result.FailureReason ?? string.Empty
        };
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request, ServerCallContext context)
    {
        _logger.LogDebug("ReleaseStock called for OrderId: {OrderId}", request.OrderId);

        var command = new ReleaseStockCommand(Guid.Parse(request.OrderId));
        var result = await _mediator.Send(command, context.CancellationToken);

        return new ReleaseStockResponse
        {
            Success = result.Success,
            FailureReason = result.FailureReason ?? string.Empty
        };
    }
}
```

---

## 4. Stock Reservation Flow

### 4.1 Reserve Stock

```
1. Order Service calls ReserveStock(orderId, items[])
2. Product Service validates request
3. For each item:
   a. Find product by ID
   b. Check if stockQuantity >= requested quantity
   c. If insufficient: return failure with failed product IDs
4. If all items available:
   a. Decrease stockQuantity for each product
   b. Create StockReservation record (linked to orderId)
   c. Check if any product fell below lowStockThreshold
   d. If below threshold: queue StockLow event (via Outbox)
5. Return success response
```

### 4.2 Release Stock

```
1. Order Service calls ReleaseStock(orderId)
2. Product Service finds StockReservation by orderId
3. For each reserved item:
   a. Increase stockQuantity back
   b. Mark reservation as released
4. Return success response
```

---

## 5. Domain Model

### 5.1 Product Aggregate

```csharp
public class ProductEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public string Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public uint Version { get; private set; } // optimistic concurrency

    public bool ReserveStock(int quantity)
    {
        if (StockQuantity < quantity)
            return false;

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLowStock => StockQuantity <= LowStockThreshold;
}
```

### 5.2 Stock Reservation

```csharp
public class StockReservationEntity
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public EReservationStatus Status { get; private set; }

    public static StockReservationEntity Create(Guid orderId, Guid productId, int quantity)
    {
        return new StockReservationEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // TTL
            Status = EReservationStatus.Active
        };
    }

    public void Release()
    {
        Status = EReservationStatus.Released;
        ReleasedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = EReservationStatus.Expired;
    }
}

public enum EReservationStatus
{
    Active,
    Released,
    Expired
}
```

---

## 6. Stock Reservation Expiration

### 6.1 Purpose

Prevents orphan reservations when Order Service fails after stock was reserved but before the order was persisted. Reservations automatically expire after 15 minutes.

### 6.2 Background Job

```csharp
// Product.Infrastructure/BackgroundJobs/StockReservationExpirationJob.cs
public class StockReservationExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IProductDbContext>();
            var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

            var expiredReservations = await db.StockReservations
                .Where(r => r.Status == EReservationStatus.Active)
                .Where(r => r.ExpiresAt < DateTime.UtcNow)
                .Include(r => r.Product)
                .ToListAsync(ct);

            foreach (var reservation in expiredReservations)
            {
                // Release stock back to inventory (reservation.Product is ProductEntity)
                reservation.Product.ReleaseStock(reservation.Quantity);
                reservation.Expire();

                // Notify interested parties (optional)
                await outbox.AddAsync(new StockReservationExpiredEvent(
                    reservation.OrderId,
                    reservation.ProductId,
                    reservation.Quantity));
            }

            if (expiredReservations.Any())
                await db.SaveChangesAsync(ct);

            await Task.Delay(_checkInterval, ct);
        }
    }
}
```

### 6.3 Registration

```csharp
// Product.API/Program.cs
builder.Services.AddHostedService<StockReservationExpirationJob>();
```

---

## 7. Idempotency Guarantees

### 7.1 ReserveStock Idempotency

`ReserveStock` operation is idempotent - calling it multiple times with the same `OrderId` returns success without reserving stock twice.

```csharp
public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    public async Task<ReserveStockResult> Handle(ReserveStockCommand cmd, CancellationToken ct)
    {
        // Idempotency check - return existing reservation if found
        var existingReservation = await _db.StockReservations
            .FirstOrDefaultAsync(r => r.OrderId == cmd.OrderId, ct);

        if (existingReservation != null)
        {
            return existingReservation.Status == EReservationStatus.Active
                ? ReserveStockResult.Success()
                : ReserveStockResult.AlreadyProcessed(existingReservation.Status.ToString());
        }

        // ... proceed with new reservation ...
    }
}
```

### 7.2 Why This Matters

In distributed systems, network failures can cause uncertainty:

```
Order Service ──ReserveStock──► Product Service
                    │
                    ├─ Request succeeds, response lost (timeout)
                    │  Order Service retries
                    │  Without idempotency: stock reserved twice!
                    │
                    └─ With idempotency: second call returns existing reservation
```

---

## 8. Published Events

| Event | Trigger | Data |
|-------|---------|------|
| `StockLow` | Stock falls below threshold | ProductId, ProductName, CurrentQuantity, Threshold |
| `StockReservationExpired` | Reservation TTL exceeded | OrderId, ProductId, Quantity |
| `ProductCreated` | New product added | ProductId, Name, Price |
| `ProductUpdated` | Product details changed | ProductId, Name, Price |

See [Messaging Communication](./messaging-communication.md) for event definitions.

---

## 9. Project Structure

```
Product.API/
├── Controllers/
│   └── ProductsController.cs        # External API (/api/products)
├── Grpc/                            # gRPC service implementations
│   └── ProductGrpcService.cs
└── Program.cs

Product.Application/
├── Commands/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductCommandHandler.cs
│   │   └── CreateProductCommandValidator.cs
│   ├── UpdateProduct/
│   ├── ReserveStock/
│   │   ├── ReserveStockCommand.cs
│   │   └── ReserveStockCommandHandler.cs
│   └── ReleaseStock/
├── Queries/
│   ├── GetProducts/
│   ├── GetProductById/
│   └── GetProductsBatch/            # Internal API - batch query
│       ├── GetProductsBatchQuery.cs
│       └── GetProductsBatchQueryHandler.cs
├── Data/
│   └── IProductDbContext.cs         # DbContext interface (NO Repository)
└── Behaviors/                       # (from EShop.Common)

Product.Domain/
├── Entities/
│   ├── ProductEntity.cs             # Inherits from Entity (EShop.SharedKernel)
│   └── StockReservationEntity.cs
├── Enums/
│   └── EEReservationStatus.cs        # Following naming convention
├── Events/
│   └── StockReservedDomainEvent.cs
└── Exceptions/
    └── InsufficientStockException.cs

Product.Infrastructure/
├── Data/
│   ├── ProductDbContext.cs          # Implements IProductDbContext
│   └── Configurations/
│       ├── ProductConfiguration.cs
│       └── StockReservationConfiguration.cs
└── Outbox/                          # Uses base from EShop.Common
    └── OutboxProcessor.cs
```

### 9.1 DbContext Interface Pattern

Instead of Repository pattern, we use direct DbContext access via interface:

```csharp
// Product.Application/Data/IProductDbContext.cs
public interface IProductDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<StockReservationEntity> StockReservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Product.Infrastructure/Data/ProductDbContext.cs
public class ProductDbContext : DbContext, IProductDbContext
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<StockReservationEntity> StockReservations => Set<StockReservationEntity>();

    // EF Core configuration...
}
```

### 9.2 Handler Example (using DbContext)

```csharp
public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    private readonly IProductDbContext _db;
    private readonly IOutboxRepository _outbox;

    public ReserveStockCommandHandler(IProductDbContext db, IOutboxRepository outbox)
    {
        _db = db;
        _outbox = outbox;
    }

    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken ct)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        // Business logic...
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (!product.ReserveStock(item.Quantity))
                return ReserveStockResult.Failure("Insufficient stock");

            _db.StockReservations.Add(StockReservationEntity.Create(request.OrderId, product.Id, item.Quantity));
        }

        await _db.SaveChangesAsync(ct);
        return ReserveStockResult.Success();
    }
}
```

---

## 10. Database Schema

### Products Table

```sql
CREATE TABLE Products (
    Id UUID PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(2000),
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    LowStockThreshold INT NOT NULL DEFAULT 10,
    Category VARCHAR(100),
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    Version INT NOT NULL DEFAULT 1
);
```

### Stock Reservations Table

```sql
CREATE TABLE StockReservations (
    Id UUID PRIMARY KEY,
    OrderId UUID NOT NULL,
    ProductId UUID NOT NULL REFERENCES Products(Id),
    Quantity INT NOT NULL,
    ReservedAt TIMESTAMP NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    ReleasedAt TIMESTAMP,
    Status VARCHAR(20) NOT NULL,

    INDEX IX_StockReservations_OrderId (OrderId),
    INDEX IX_StockReservations_ProductId (ProductId),
    INDEX IX_StockReservations_Expiration (Status, ExpiresAt) WHERE Status = 'Active'
);
```

---

## Related Documents

- [Internal API Communication](./internal-api-communication.md) - Internal API layer concept
- [gRPC Communication](./grpc-communication.md) - gRPC technical patterns
- [Messaging Communication](./messaging-communication.md) - Event publishing
- [Order Service Interface](./order-service-interface.md) - gRPC client (consumes this API)
- [CorrelationId Flow](./correlation-id-flow.md) - Request tracing
