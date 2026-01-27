# Task 03: Stock Reservation Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | :white_circle: pending |
| Dependencies | task-01 |

## Summary
Integrate IProductServiceClient.ReserveStockAsync in CreateOrderCommandHandler to reserve stock when creating orders.

## Scope
- [ ] Inject IProductServiceClient into CreateOrderCommandHandler
- [ ] Call ReserveStockAsync for each order item before creating order
- [ ] Handle reservation failures (throw domain exception or return error)
- [ ] Implement compensation logic if partial reservation fails (release already reserved items)
- [ ] Add unit tests for reservation success/failure scenarios

## Reference Implementation
See `src/Services/Products/Products.Application/Features/Stock/Commands/` for stock command patterns.

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Stock Operations)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: Create Order Flow)

---
## Notes
(Updated during implementation)
