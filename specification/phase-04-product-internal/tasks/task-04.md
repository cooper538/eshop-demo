# Task 04: ReserveStock and ReleaseStock Commands

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-02, task-03 |

## Summary
Implement CQRS commands for stock reservation and release operations with idempotency support.

## Scope
- [x] Create ReserveStockCommand record
- [x] Implement ReserveStockCommandHandler
- [x] Add idempotency check (existing reservation for same OrderId returns success)
- [x] Handle insufficient stock scenario
- [x] Create StockReservation records for each item
- [x] Update Product.StockQuantity on reserve
- [x] Create ReleaseStockCommand record
- [x] Implement ReleaseStockCommandHandler
- [x] Find reservations by OrderId and release them
- [x] Update Product.StockQuantity on release
- [x] Create result types (StockReservationResult, StockReleaseResult)

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 4: Stock Reservation Flow, Section 7: Idempotency Guarantees)

---
## Notes
(Updated during implementation)
