# Phase 6: Order Service Integration

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Integrate Order Service with Product Service via gRPC communication for stock management.

## Scope
- [x] Configure gRPC client in ServiceClients for Order Service
- [x] Configure resilience policies (gRPC built-in retry with exponential backoff)
- [x] Integrate stock reservation into CreateOrder flow
- [x] Integrate stock release into CancelOrder flow
- [x] Add CorrelationId propagation via gRPC interceptors

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | Register ServiceClients in Order Service | ✅ | - |
| 2 | task-02 | Resilience Configuration | ✅ | task-01 |
| 3 | task-03 | Stock Reservation Integration | ✅ | task-01 |
| 4 | task-04 | Stock Release Integration | ✅ | task-03 |
| 5 | task-05 | CorrelationId Client Interceptor | ✅ | task-01 |

## Implementation Summary

### Architecture
```
Order.API
  └── AddPresentation()
        └── AddServiceClients() ─── registers IProductServiceClient
              ├── GrpcProductServiceClient (ReserveStock, ReleaseStock)
              ├── CorrelationIdClientInterceptor (propagates correlation ID)
              └── LoggingInterceptor (logs gRPC calls)
```

### Key Files
- `src/Services/Order/Order.API/DependencyInjection.cs` - ServiceClients registration
- `src/Services/Order/Order.Application/Commands/CreateOrder/CreateOrderCommandHandler.cs` - Stock reservation
- `src/Services/Order/Order.Application/Commands/CancelOrder/CancelOrderCommandHandler.cs` - Stock release
- `src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs` - gRPC client setup
- `src/Common/EShop.ServiceClients/Infrastructure/Grpc/CorrelationIdClientInterceptor.cs` - CorrelationId propagation

## Related Specs
- [grpc-communication.md](../high-level-specs/grpc-communication.md)
- [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md)

---
## Notes
- Uses gRPC built-in retry policy instead of Polly circuit breaker (simpler, sufficient for this use case)
- Stock release failures are logged but don't fail the order cancellation (graceful degradation)
