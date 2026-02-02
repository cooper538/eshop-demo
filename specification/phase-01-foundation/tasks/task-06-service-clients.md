# Task 6: EShop.ServiceClients

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-01, task-03, task-04, task-05 |

## Summary
Implement gRPC client abstraction for inter-service communication.

## Scope
- [x] Create project `EShop.ServiceClients` in `src/Common/EShop.ServiceClients/`
- [x] Implement Product client:
  - `Clients/Product/GrpcProductServiceClient.cs` - gRPC implementation of IProductServiceClient
  - `Clients/Product/Mappers/` - Mapperly mappers for request/result conversion:
    - `GetProductsResponseMapper.cs`
    - `ReserveStockRequestMapper.cs`
    - `StockReservationResultMapper.cs`
    - `ReleaseStockRequestMapper.cs`
    - `StockReleaseResultMapper.cs`
- [x] Configuration in `Configuration/`:
  - `ServiceClientOptions.cs` - service endpoint configuration
  - `ServiceEndpoints.cs` - endpoint URLs
  - `ResilienceOptions.cs` - retry/timeout settings
- [x] Exception handling in `Exceptions/`:
  - `ServiceClientException.cs`
  - `EServiceClientErrorCode.cs` - error codes enum
- [x] gRPC client interceptors in `Infrastructure/Grpc/`:
  - `CorrelationIdClientInterceptor.cs` - adds correlation ID to outgoing calls
  - `LoggingInterceptor.cs` - request/response logging
  - `GrpcExtensions.cs` - helper methods
- [x] `Infrastructure/ServiceClientLog.cs` - structured logging
- [x] `Extensions/ServiceCollectionExtensions.cs` - DI registration with:
  - Service discovery integration
  - Resilience policies (retry, circuit breaker)
  - Interceptor registration

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.5 - EShop.ServiceClients)
- -> [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: 4, 5 - Client patterns, Resiliency)

---
## Notes
- Interface (IProductServiceClient) is in EShop.Contracts, implementation here
- Uses Mapperly for compile-time mapping between gRPC and domain models
- Integrates with .NET Aspire service discovery via Microsoft.Extensions.ServiceDiscovery
- Dependencies updated to include task-03 (interface now in Contracts)
