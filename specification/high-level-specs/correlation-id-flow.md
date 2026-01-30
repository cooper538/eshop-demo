# CorrelationId Flow Architecture

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Distributed request tracing |
| Header | `X-Correlation-ID` |
| Format | GUID (UUID v4) |
| Propagation | **Implicit** - HTTP middleware → gRPC interceptors → MassTransit filters |

---

## 1. Overview

CorrelationId enables end-to-end request tracing across all services. The correlation ID is propagated **implicitly** through ambient context - developers don't need to manually pass it.

### Design Principles

- **Implicit propagation** - Cannot be forgotten, handled automatically by infrastructure
- **Ambient context** - `AsyncLocal<T>` flows across async boundaries
- **Separation of concerns** - Business code doesn't see correlation ID plumbing
- **5xx errors only** - Correlation ID returned to users only for server errors

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    IMPLICIT CORRELATION ID FLOW                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │  AMBIENT CONTEXT (AsyncLocal<T>)                                    │ │
│  │  CorrelationContext.Current.CorrelationId = "abc-123-def-456"      │ │
│  │  (Available everywhere via ICorrelationIdAccessor)                  │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│           │                      │                       │               │
│           ▼                      ▼                       ▼               │
│  ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────────┐   │
│  │ HTTP Middleware │   │ gRPC Interceptor│   │ MassTransit Filter  │   │
│  │ (entry point)   │   │ (client+server) │   │ (publish+consume)   │   │
│  │                 │   │                 │   │                     │   │
│  │ X-Correlation-ID│   │ x-correlation-id│   │ X-Correlation-ID    │   │
│  │ (HTTP header)   │   │ (gRPC metadata) │   │ (message header)    │   │
│  └─────────────────┘   └─────────────────┘   └─────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. End-to-End Flow

### 2.1 Complete Request Trace

```
1. Client sends POST /api/orders
   └─ Optional: X-Correlation-ID header (if not provided, generated)

2. API Gateway (CorrelationIdMiddleware)
   └─ Extracts or generates: X-Correlation-ID: "abc-123-def-456"
   └─ Sets CorrelationContext.Current (ambient context)
   └─ Adds to response headers
   └─ Adds to logging scope

3. Order Service
   └─ CorrelationContext.Current already set (from middleware)
   └─ All logs automatically include CorrelationId
   └─ Calls Product Service via gRPC
      └─ Client interceptor AUTOMATICALLY adds to gRPC metadata

4. Product Service
   └─ Server interceptor AUTOMATICALLY extracts from gRPC metadata
   └─ Sets CorrelationContext.Current
   └─ All logs automatically include CorrelationId
   └─ Returns gRPC response

5. Order Service (continued)
   └─ Publishes OrderConfirmed event
      └─ Publish filter AUTOMATICALLY adds to message headers

6. Notification Service
   └─ Consume filter AUTOMATICALLY extracts from message headers
   └─ Sets CorrelationContext.Current
   └─ All logs automatically include CorrelationId
   └─ Sends email notification
```

### 2.2 Log Output Example

All services use the same CorrelationId automatically:

```
[2024-01-15 10:30:00] [Gateway] [abc-123-def-456] Received POST /api/orders
[2024-01-15 10:30:00] [Order]   [abc-123-def-456] Creating order for customer 123
[2024-01-15 10:30:00] [Order]   [abc-123-def-456] Calling ProductService.ReserveStock
[2024-01-15 10:30:01] [Product] [abc-123-def-456] ReserveStock called for OrderId: 550e8400
[2024-01-15 10:30:01] [Product] [abc-123-def-456] Stock reserved successfully
[2024-01-15 10:30:01] [Order]   [abc-123-def-456] Order confirmed, publishing event
[2024-01-15 10:30:02] [Notif]   [abc-123-def-456] Processing OrderConfirmed event
[2024-01-15 10:30:02] [Notif]   [abc-123-def-456] Email sent to customer@example.com
```

---

## 3. Ambient Context

### 3.1 CorrelationContext

Thread-safe ambient context using `AsyncLocal<T>`:

```csharp
public sealed class CorrelationContext
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public string CorrelationId { get; }

    private CorrelationContext(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public static CorrelationContext? Current
    {
        get => CurrentContext.Value;
        private set => CurrentContext.Value = value;
    }

    public static IDisposable CreateScope(string correlationId)
    {
        var previous = Current;
        Current = new CorrelationContext(correlationId);
        return new CorrelationScope(previous);
    }

    private sealed class CorrelationScope : IDisposable
    {
        private readonly CorrelationContext? _previous;
        private bool _disposed;

        public CorrelationScope(CorrelationContext? previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed) return;
            Current = _previous;
            _disposed = true;
        }
    }
}
```

### 3.2 ICorrelationIdAccessor

Interface for accessing correlation ID from any context:

```csharp
public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
}

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId =>
        CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();
}
```

### 3.3 Constants

```csharp
public static class CorrelationIdConstants
{
    public const string HttpHeaderName = "X-Correlation-ID";
    public const string GrpcMetadataKey = "x-correlation-id";
    public const string MassTransitHeaderKey = "X-Correlation-ID";
    public const string LoggingScopeKey = "CorrelationId";
}
```

---

## 4. HTTP Layer (Entry Point)

### 4.1 CorrelationId Middleware

```csharp
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract from header or generate new
        var correlationId = context.Request.Headers[CorrelationIdConstants.HttpHeaderName]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        // Add to response headers (always, for debugging)
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdConstants.HttpHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Create ambient context scope + logging scope
        using (CorrelationContext.CreateScope(correlationId))
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdConstants.LoggingScopeKey] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

### 4.2 Registration

```csharp
app.UseCorrelationId();  // Early in pipeline, before routing
```

### 4.3 Accessing CorrelationId in Services

**No manual extraction needed!** Just inject `ICorrelationIdAccessor`:

```csharp
public class OrdersController : ControllerBase
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        // CorrelationId is automatically available
        _logger.LogInformation("Creating order"); // Log already includes CorrelationId

        // If you need the value explicitly:
        var correlationId = _correlationIdAccessor.CorrelationId;
    }
}
```

---

## 5. gRPC Layer (Implicit via Interceptors)

### 5.1 Proto Definition

**No correlation_id field in messages!** Propagated via metadata:

```protobuf
message ReserveStockRequest {
  string order_id = 1;
  reserved 2;  // Was: correlation_id - now propagated via gRPC metadata
  repeated OrderItem items = 3;
}

message ReleaseStockRequest {
  string order_id = 1;
  reserved 2;  // Was: correlation_id - now propagated via gRPC metadata
}
```

### 5.2 Client Interceptor (Outgoing Calls)

Automatically adds correlation ID to all outgoing gRPC calls:

```csharp
public sealed class CorrelationIdClientInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var correlationId = CorrelationContext.Current?.CorrelationId
            ?? Guid.NewGuid().ToString();

        var metadata = context.Options.Headers ?? new Metadata();
        metadata.Add(CorrelationIdConstants.GrpcMetadataKey, correlationId);

        var newOptions = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }
}
```

### 5.3 Server Interceptor (Incoming Calls)

Automatically extracts and sets ambient context:

```csharp
public sealed class CorrelationIdServerInterceptor : Interceptor
{
    private readonly ILogger<CorrelationIdServerInterceptor> _logger;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders
            .FirstOrDefault(h => h.Key == CorrelationIdConstants.GrpcMetadataKey)?.Value
            ?? Guid.NewGuid().ToString();

        using (CorrelationContext.CreateScope(correlationId))
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdConstants.LoggingScopeKey] = correlationId
        }))
        {
            return await continuation(request, context);
        }
    }
}
```

### 5.4 Registration

```csharp
// Client (Order Service)
builder.Services
    .AddGrpcClient<ProductService.ProductServiceClient>(...)
    .AddCorrelationIdInterceptor();

// Server (Product Service)
builder.Services.AddGrpc().AddCorrelationIdInterceptor();
```

### 5.5 Usage - No Changes Needed!

```csharp
// Business code doesn't touch correlation ID
public async Task<StockReservationResult> ReserveStockAsync(
    Guid orderId, IEnumerable<OrderItemDto> items, CancellationToken ct)
{
    var request = new ReserveStockRequest
    {
        OrderId = orderId.ToString()
        // NO correlation_id field - handled by interceptor!
    };
    request.Items.AddRange(items.Select(...));

    var response = await _client.ReserveStockAsync(request, cancellationToken: ct);
    return new StockReservationResult(response.Success, response.FailureReason);
}
```

---

## 6. Messaging Layer (Implicit via Filters)

### 6.1 Event Definition

**No CorrelationId property!** Propagated via message headers:

```csharp
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    // CorrelationId REMOVED - now propagated via message headers
}

public record OrderConfirmed : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    // ...
}
```

### 6.2 Publish Filter

Automatically adds correlation ID to all published messages:

```csharp
public sealed class CorrelationIdPublishFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var correlationId = CorrelationContext.Current?.CorrelationId
            ?? Guid.NewGuid().ToString();

        context.Headers.Set(CorrelationIdConstants.MassTransitHeaderKey, correlationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationIdPublishFilter");
}
```

### 6.3 Consume Filter

Automatically extracts and sets ambient context:

```csharp
public sealed class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<CorrelationIdConsumeFilter<T>> _logger;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.Headers.Get<string>(CorrelationIdConstants.MassTransitHeaderKey)
            ?? context.CorrelationId?.ToString()  // Fallback to MassTransit built-in
            ?? Guid.NewGuid().ToString();

        using (CorrelationContext.CreateScope(correlationId))
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdConstants.LoggingScopeKey] = correlationId
        }))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationIdConsumeFilter");
}
```

### 6.4 Registration

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(...);
        cfg.UseCorrelationIdFilters();  // Adds publish + consume filters
        cfg.ConfigureEndpoints(context);
    });
});
```

### 6.5 Usage - No Changes Needed!

```csharp
// Publishing - no CorrelationId needed
var @event = new OrderConfirmed
{
    OrderId = order.Id,
    CustomerId = order.CustomerId
    // NO CorrelationId property - handled by filter!
};
await _publishEndpoint.Publish(@event, ct);

// Consuming - CorrelationId automatically in logging scope
public async Task Consume(ConsumeContext<OrderConfirmed> context)
{
    _logger.LogInformation("Processing order {OrderId}", context.Message.OrderId);
    // Log automatically includes CorrelationId!
}
```

---

## 7. Error Responses (5xx Only)

### 7.1 Exception Handler

Adds correlation ID to ProblemDetails **only for 5xx errors**:

```csharp
public sealed class CorrelationIdExceptionHandler : IExceptionHandler
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly ILogger<CorrelationIdExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdAccessor.CorrelationId;

        _logger.LogError(exception,
            "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        // Only include correlation ID for 5xx errors
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### 7.2 Response Examples

**5xx Error (includes correlationId):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Database connection failed",
  "instance": "/api/orders",
  "correlationId": "abc-123-def-456"
}
```

**4xx Error (no correlationId):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Order must contain at least one item",
  "instance": "/api/orders"
}
```

---

## 8. Logging Configuration

### 8.1 Structured Logging with CorrelationId

```csharp
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;  // Important: includes BeginScope properties
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
```

### 8.2 Log Output Format

```json
{
  "timestamp": "2024-01-15 10:30:00",
  "level": "Information",
  "message": "ReserveStock called for OrderId: 550e8400",
  "CorrelationId": "abc-123-def-456",
  "OrderId": "550e8400-e29b-41d4-a716-446655440000",
  "service": "Product"
}
```

---

## 9. Service Registration Summary

```csharp
// Program.cs (all services)

// 1. Add correlation ID services
builder.Services.AddCorrelationId();

// 2. gRPC client (if calling other services)
builder.Services
    .AddGrpcClient<ProductService.ProductServiceClient>(...)
    .AddCorrelationIdInterceptor();

// 3. gRPC server (if exposing gRPC endpoints)
builder.Services.AddGrpc().AddCorrelationIdInterceptor();

// 4. MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseCorrelationIdFilters();
        cfg.ConfigureEndpoints(context);
    });
});

// 5. Middleware (must be early in pipeline)
app.UseCorrelationId();
```

---

## 10. File Structure

```
src/EShop.Common/
├── Correlation/
│   ├── CorrelationContext.cs
│   ├── CorrelationIdAccessor.cs
│   ├── CorrelationIdConstants.cs
│   ├── CorrelationServiceCollectionExtensions.cs
│   ├── ICorrelationIdAccessor.cs
│   ├── Grpc/
│   │   ├── CorrelationIdClientInterceptor.cs
│   │   ├── CorrelationIdServerInterceptor.cs
│   │   └── GrpcCorrelationExtensions.cs
│   ├── Http/
│   │   ├── CorrelationIdExceptionHandler.cs
│   │   ├── CorrelationIdMiddleware.cs
│   │   └── CorrelationIdMiddlewareExtensions.cs
│   └── MassTransit/
│       ├── CorrelationIdConsumeFilter.cs
│       ├── CorrelationIdPublishFilter.cs
│       ├── CorrelationIdSendFilter.cs
│       └── MassTransitCorrelationExtensions.cs
```

---

## Related Documents

- [gRPC Communication](./grpc-communication.md) - gRPC interceptor details
- [Messaging Communication](./messaging-communication.md) - MassTransit filter details
- [Order Service Interface](./order-service-interface.md) - HTTP layer usage
- [Product Service Interface](./product-service-interface.md) - gRPC server usage
