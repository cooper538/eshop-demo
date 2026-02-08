# Task 04: Client Contract and Implementation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ⬜ pending |
| Dependencies | task-01 |

## Summary
Update `IProductServiceClient` interface and `GrpcProductServiceClient` implementation to consume the server-side stream, returning `IAsyncEnumerable<ProductInfo>` instead of `Task<GetProductsResult>`.

## Scope
- [ ] Change `IProductServiceClient.GetAllProductsAsync` return type to `IAsyncEnumerable<ProductInfo>` in `src/Common/EShop.Contracts/`
- [ ] Update `GrpcProductServiceClient` to use `call.ResponseStream.ReadAllAsync()` + `yield return` for streaming consumption
- [ ] Create `ProductInfoMapper.cs` extension method for single-item mapping (replaces `GetProductsResponseMapper`)

## Related Specs
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Client Configuration)
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section: Service Client Interface)

---
## Notes
(Updated during implementation)
