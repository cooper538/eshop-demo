# Task 05: Consumer Batched Saves and Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⬜ pending |
| Dependencies | task-04 |

## Summary
Convert `ProductSnapshotSyncJob` from `Task<GetProductsResult>` consumption to `await foreach` with `IAsyncEnumerable`, adding 500-item batch saves, and update all affected unit tests.

## Scope
- [ ] Change `ProductSnapshotSyncJob` from `await result.Products` loop to `await foreach` over `IAsyncEnumerable<ProductInfo>`
- [ ] Implement batch saves: buffer 500 snapshots, `AddRange` + `SaveChangesAsync` + `ChangeTracker.Clear()` per batch
- [ ] File: `src/Services/Order/Order.Infrastructure/BackgroundJobs/ProductSnapshotSyncJob.cs`
- [ ] Update 4 unit tests in `ProductSnapshotSyncJobTests.cs` -- mock `IAsyncEnumerable<ProductInfo>` instead of `Task<GetProductsResult>`
- [ ] File: `tests/Order.UnitTests/Infrastructure/BackgroundJobs/ProductSnapshotSyncJobTests.cs`

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section: Service Client Interface)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Client Configuration)

---
## Notes
(Updated during implementation)
