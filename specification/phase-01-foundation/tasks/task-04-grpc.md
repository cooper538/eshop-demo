# Task 4: EShop.Grpc

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Create proto definitions and project for gRPC communication.

## Scope
- [x] Create project `EShop.Grpc` in `src/Common/EShop.Grpc/`
- [x] Configure `.csproj` with `GrpcServices="Both"` for client and server code generation
- [x] Implement `Protos/product.proto`:
  - `ProductService` with methods:
    - `GetProducts` - batch get product info (fails if any missing)
    - `ReserveStock` - all-or-nothing stock reservation
    - `ReleaseStock` - idempotent release
  - Messages:
    - `GetProductsRequest/Response` with `ProductInfo`
    - `ReserveStockRequest/Response` with `OrderItem`
    - `ReleaseStockRequest/Response`
- [x] Verify C# code generation works

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.3 - EShop.Grpc)
- -> [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: 2 - Project Setup)

---
## Notes
- Added GetProducts RPC for batch product retrieval (used by Order service for validation)
- ProductInfo message includes product_id, name, description, price (as string for decimal), stock_quantity
- ReleaseStock only needs order_id (reservation tracked by order)
