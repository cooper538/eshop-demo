# Task 4: EShop.Grpc

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Vytvořit proto definice a projekt pro gRPC komunikaci.

## Scope
- [ ] Vytvořit projekt `EShop.Grpc` v `src/Common/EShop.Grpc/`
- [ ] Nakonfigurovat `.csproj` s `GrpcServices="Both"` pro generování client i server kódu
- [ ] Implementovat `Protos/product.proto`:
  - `ProductService` s metodami `ReserveStock`, `ReleaseStock`
  - `ReserveStockRequest/Response` messages
  - `ReleaseStockRequest/Response` messages
  - `OrderItem` message
- [ ] Ověřit generování C# kódu

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.3 - EShop.Grpc)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: 2 - Project Setup)

---
## Notes
(Updated during implementation)
