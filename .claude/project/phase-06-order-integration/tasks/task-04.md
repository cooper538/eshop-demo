# Task 04: Stock Release Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | 🔵 in_progress |
| Dependencies | task-03 |

## Summary
Integrate IProductServiceClient.ReleaseStockAsync in CancelOrderCommandHandler to release reserved stock when cancelling orders.

## Scope
- [ ] Inject IProductServiceClient into CancelOrderCommandHandler
- [ ] Call ReleaseStockAsync for each order item when order is cancelled
- [ ] Handle release failures gracefully (log and continue, don't fail cancellation)
- [ ] Consider idempotency for retry scenarios
- [ ] Add unit tests for release success/failure scenarios

## Reference Implementation
See `src/Services/Products/Products.Application/Features/Stock/Commands/` for stock command patterns.

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Stock Operations)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: Cancel Order Flow)

---
## Notes
(Updated during implementation)
