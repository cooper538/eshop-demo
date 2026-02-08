# Phase 16: gRPC Server-Side Streaming

## Metadata
| Key | Value |
|-----|-------|
| Status | ⬜ pending |

## Objective
Convert `GetAllProducts` gRPC RPC from unary to server-side streaming with batched DB reads using MediatR `IStreamRequest<T>` + `CreateStream()`, enabling constant memory usage regardless of product count.

## Scope
- [ ] Change `GetAllProducts` proto definition from unary to `stream ProductInfo`, delete `GetProductsResponse` message
- [ ] Create `StreamAllProductsQuery` MediatR stream handler with `AsNoTracking().AsAsyncEnumerable()`
- [ ] Add `ServerStreamingServerHandler` override to all 4 server-side gRPC interceptors
- [ ] Add `AsyncServerStreamingCall` override to both client-side gRPC interceptors
- [ ] Update `ProductGrpcService.GetAllProducts` to streaming signature with `CreateStream()` + `responseStream.WriteAsync()`
- [ ] Update `IProductServiceClient` to return `IAsyncEnumerable<ProductInfo>`, update `GrpcProductServiceClient` with `ReadAllAsync()`
- [ ] Convert `ProductSnapshotSyncJob` to `await foreach` with 500-item batch saves and `ChangeTracker.Clear()`
- [ ] Update unit tests for `IAsyncEnumerable` mocking
- [ ] Delete obsolete batch query, result, DTO, and mapper files

## Tasks

| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Proto change and MediatR stream handler](tasks/task-01-proto-and-stream-handler.md) | ⬜ | - |
| 02 | [gRPC interceptors streaming support](tasks/task-02-grpc-interceptors-streaming.md) | ⬜ | - |
| 03 | [Server gRPC handler update](tasks/task-03-server-grpc-handler.md) | ⬜ | 01, 02 |
| 04 | [Client contract and implementation](tasks/task-04-client-contract-and-implementation.md) | ⬜ | 01 |
| 05 | [Consumer batched saves and tests](tasks/task-05-consumer-batched-saves-and-tests.md) | ⬜ | 04 |
| 06 | [Dead code cleanup](tasks/task-06-dead-code-cleanup.md) | ⬜ | 01, 02, 03, 04, 05 |

## Related Specs
- → [grpc-communication.md](../high-level-specs/grpc-communication.md) (streaming pattern, interceptors)
- → [product-service-interface.md](../high-level-specs/product-service-interface.md) (GetAllProducts RPC)

---
## Notes
- Problem: unary `GetAllProducts` loads all products via `ToListAsync()` -- ~40MB memory spikes at 10k+ products, risks 4MB gRPC message limit
- Solution: server-side streaming with `AsAsyncEnumerable()` -- constant memory from DB cursor through gRPC to client
