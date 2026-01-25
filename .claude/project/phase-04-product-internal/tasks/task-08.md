# Task 08: Stock Domain Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | :white_circle: pending |
| Dependencies | task-04 |

## Summary
Implement domain events for stock operations to enable eventual consistency and event-driven coordination within Product bounded context.

## Scope
- [ ] Create StockReservedDomainEvent (OrderId, ProductId, Quantity, ReservedAt)
- [ ] Create StockReleasedDomainEvent (OrderId, ProductId, Quantity, ReleasedAt)
- [ ] Create StockReservationExpiredDomainEvent (OrderId, ProductId, Quantity)
- [ ] Raise StockReservedDomainEvent in ReserveStockCommandHandler after successful reservation
- [ ] Raise StockReleasedDomainEvent in ReleaseStockCommandHandler after successful release
- [ ] Raise StockReservationExpiredDomainEvent in StockReservationExpirationJob
- [ ] Ensure domain events are dispatched via MediatR INotificationHandler pattern

## Notes
Domain events are internal to the bounded context. For cross-service communication, these can trigger integration events (published via Outbox) in Phase 7.

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 8: Published Events, Section 9: Project Structure)

---
## Notes
(Updated during implementation)
