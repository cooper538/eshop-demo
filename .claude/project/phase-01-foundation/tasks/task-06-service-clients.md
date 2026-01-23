# Task 6: EShop.ServiceClients

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-01, task-04, task-05 |

## Summary
Implementovat dual-protocol abstrakci pro inter-service komunikaci.

## Scope
- [x] Vytvořit projekt `EShop.ServiceClients` v `src/Common/EShop.ServiceClients/`
- [x] Implementovat `Abstractions/IProductServiceClient.cs` - interface pro ProductService
- [x] Implementovat request/result modely v `Models/`:
  - `ReserveStockRequest.cs`
  - `StockReservationResult.cs`
  - `ReleaseStockRequest.cs`
  - `StockReleaseResult.cs`
- [x] Implementovat `Grpc/GrpcProductServiceClient.cs` - gRPC implementace
- [x] Implementovat `Http/HttpProductServiceClient.cs` - HTTP fallback implementace
- [x] Implementovat `Configuration/ServiceClientOptions.cs` - konfigurace pro protocol switching
- [x] Implementovat `Exceptions/ServiceClientException.cs`
- [x] Implementovat `Resilience/ResiliencePolicies.cs` - Polly policies (retry, circuit breaker)
- [x] Implementovat `Extensions/ServiceCollectionExtensions.cs` - DI registrace s protocol factory

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.5 - EShop.ServiceClients)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: 4, 5 - Client patterns, Resiliency)

---
## Notes
(Updated during implementation)
