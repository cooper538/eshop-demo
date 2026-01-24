# Task 04: ReserveStock and ReleaseStock Commands

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | :white_circle: pending |
| Dependencies | task-02, task-03 |

## Summary
Implement CQRS commands for stock reservation and release operations with idempotency support.

## Scope
- [ ] Create ReserveStockCommand record
- [ ] Implement ReserveStockCommandHandler
- [ ] Add idempotency check (existing reservation for same OrderId returns success)
- [ ] Handle insufficient stock scenario
- [ ] Create StockReservation records for each item
- [ ] Update Product.StockQuantity on reserve
- [ ] Create ReleaseStockCommand record
- [ ] Implement ReleaseStockCommandHandler
- [ ] Find reservations by OrderId and release them
- [ ] Update Product.StockQuantity on release
- [ ] Create result types (StockReservationResult, StockReleaseResult)

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 4: Stock Reservation Flow, Section 7: Idempotency Guarantees)

---
## Notes
(Updated during implementation)
