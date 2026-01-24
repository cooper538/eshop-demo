# Task 07: Stock Reservation Expiration Job

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | :white_circle: pending |
| Dependencies | task-03, task-04 |

## Summary
Background job that expires stale stock reservations and releases stock back to inventory.

## Scope
- [ ] Create StockReservationExpirationJob extending BackgroundService
- [ ] Implement ExecuteAsync with 1-minute check interval
- [ ] Query for Active reservations where ExpiresAt < UtcNow
- [ ] Release stock back to Product.StockQuantity for each expired reservation
- [ ] Update reservation status to Expired
- [ ] Add proper logging for expired reservations
- [ ] Register as hosted service in Program.cs

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 6: Stock Reservation Expiration)

---
## Notes
(Updated during implementation)
