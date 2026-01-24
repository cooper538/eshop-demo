# Task 6: EShop.ServiceClients

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-01, task-04, task-05 |

## Summary
Implement gRPC client abstraction for inter-service communication.

## Scope
- [x] Create project `EShop.ServiceClients` in `src/Common/EShop.ServiceClients/`
- [x] Implement `Clients/Product/IProductServiceClient.cs` - interface for ProductService
- [x] Implement request/result models in `Clients/Product/Models/`:
  - `ReserveStockRequest.cs`
  - `StockReservationResult.cs`
  - `ReleaseStockRequest.cs`
  - `StockReleaseResult.cs`
- [x] Implement `Clients/Product/GrpcProductServiceClient.cs` - gRPC implementation
- [x] Implement `Configuration/ServiceClientOptions.cs` - service endpoint configuration
- [x] Implement `Exceptions/ServiceClientException.cs`
- [x] Implement gRPC interceptors (logging, resilience)
- [x] Implement `Extensions/ServiceCollectionExtensions.cs` - DI registration

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.5 - EShop.ServiceClients)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: 4, 5 - Client patterns, Resiliency)

---
## Notes
(Updated during implementation)
