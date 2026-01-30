# Task 05: CorrelationId Client Interceptor

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement and register CorrelationIdClientInterceptor in gRPC client pipeline to propagate correlation IDs across service boundaries.

## Scope
- [x] Implement CorrelationIdClientInterceptor in EShop.ServiceClients
- [x] Read CorrelationId from CorrelationContext (AsyncLocal-based)
- [x] Add CorrelationId to gRPC call metadata headers
- [x] Register interceptor in gRPC client channel configuration
- [x] Generate new CorrelationId if not present (fallback)

## Implementation

### CorrelationIdClientInterceptor
```csharp
public sealed class CorrelationIdClientInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        // Read from AsyncLocal context or generate new
        var correlationId = CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();

        // Add to gRPC metadata
        var metadata = context.Options.Headers ?? new Metadata();
        metadata.Add(CorrelationIdConstants.GrpcMetadataKey, correlationId);

        var newOptions = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }
}
```

### Registration
```csharp
// ServiceCollectionExtensions.cs
services.AddTransient<CorrelationIdClientInterceptor>();

services.AddGrpcClient<ProductService.ProductServiceClient>(...)
    .AddInterceptor<CorrelationIdClientInterceptor>()
    .AddInterceptor<LoggingInterceptor>();
```

### Correlation Flow
```
HTTP Request (X-Correlation-Id header)
    ↓
CorrelationIdMiddleware (sets CorrelationContext.Current)
    ↓
CreateOrderCommandHandler
    ↓
CorrelationIdClientInterceptor (reads CorrelationContext.Current)
    ↓
gRPC Call (x-correlation-id metadata)
    ↓
Product Service (CorrelationIdServerInterceptor extracts and sets context)
```

### Key Files
- `src/Common/EShop.ServiceClients/Infrastructure/Grpc/CorrelationIdClientInterceptor.cs`
- `src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs`
- `src/Common/EShop.Common.Application/Correlation/CorrelationContext.cs`
- `src/Common/EShop.Common.Application/Correlation/CorrelationIdConstants.cs`

## Related Specs
- [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md) (Section: gRPC Propagation)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Interceptors)

---
## Notes
CorrelationId is used for distributed tracing - enables tracking requests across Order and Product services.
