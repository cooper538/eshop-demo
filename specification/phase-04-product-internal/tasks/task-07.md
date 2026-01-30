# Task 07: Stock Reservation Expiration Job

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-03, task-04 |

## Summary
Background job that expires stale stock reservations and releases stock back to inventory.

## Scope
- [x] Create StockReservationExpirationJob extending BackgroundService
- [x] Implement ExecuteAsync with 1-minute check interval
- [x] Query for Active reservations where ExpiresAt < UtcNow
- [x] Release stock back to Product.StockQuantity for each expired reservation
- [x] Update reservation status to Expired
- [x] Add proper logging for expired reservations
- [x] Register as hosted service in Program.cs

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 6: Stock Reservation Expiration)

---
## Notes
(Updated during implementation)
