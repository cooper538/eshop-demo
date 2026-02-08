# Task 01: Proto Change and MediatR Stream Handler

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ⬜ pending |
| Dependencies | - |

## Summary
Change `GetAllProducts` proto definition to server-side streaming and create a MediatR `IStreamRequest<T>` handler that streams products from DB via `AsAsyncEnumerable()`.

## Scope
- [ ] Change `rpc GetAllProducts(GetAllProductsRequest) returns (GetProductsResponse)` to `returns (stream ProductInfo)` in `src/Common/EShop.Grpc/Protos/product.proto`
- [ ] Delete `GetProductsResponse` message from proto file
- [ ] Create `StreamAllProductsQuery : IStreamRequest<ProductInfoDto>` in `src/Services/Products/Products.Application/Queries/StreamAllProducts/`
- [ ] Create `StreamAllProductsQueryHandler : IStreamRequestHandler<StreamAllProductsQuery, ProductInfoDto>` using `AsNoTracking().AsAsyncEnumerable()` instead of `ToListAsync()`
- [ ] Verify MediatR 12.5.0 `IStreamRequest<T>` + `CreateStream()` support

## Related Specs
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Proto Definitions)
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section: Internal gRPC API)

---
## Notes
(Updated during implementation)
