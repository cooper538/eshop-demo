# Task 03: Server gRPC Handler Update

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⬜ pending |
| Dependencies | task-01, task-02 |

## Summary
Update `ProductGrpcService.GetAllProducts` to use the streaming signature, consuming the MediatR stream and writing each item to the gRPC response stream.

## Scope
- [ ] Change `GetAllProducts` method signature to accept `IServerStreamWriter<ProductInfo>` + `ServerCallContext`
- [ ] Use `_mediator.CreateStream(new StreamAllProductsQuery(), context.CancellationToken)` to get `IAsyncEnumerable`
- [ ] `await foreach` over stream, map each `ProductInfoDto` to `ProductInfo`, call `responseStream.WriteAsync()`
- [ ] Delete `MapToResponse()` helper method (no longer needed -- single-item mapping inline)
- [ ] File: `src/Services/Products/Products.API/Grpc/ProductGrpcService.cs`

## Related Specs
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: gRPC Services)
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section: Internal gRPC API)

---
## Notes
(Updated during implementation)
