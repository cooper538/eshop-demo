# Task 08: Stock Domain Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ✅ completed |
| Dependencies | task-04 |

## Summary
Implement domain events for stock operations to enable eventual consistency and event-driven coordination within Product bounded context.

## Scope
- [x] Create LowStockWarningDomainEvent (ProductId, AvailableQuantity, Threshold)
- [x] Create StockReservationExpiredDomainEvent (OrderId, ProductId, Quantity)
- [x] Raise LowStockWarningDomainEvent in Stock.ReserveStock() when stock falls below threshold
- [x] Raise StockReservationExpiredDomainEvent in Stock.ExpireStaleReservations()
- [x] Implement StockReservationExpiredEventHandler (logs + TODO for integration event)
- [x] Implement LowStockWarningEventHandler (logs warning)
- [x] Ensure domain events are dispatched via MediatR INotificationHandler pattern

## Implementation Details

**Files**:
- `Products.Domain/Events/LowStockWarningDomainEvent.cs`
- `Products.Domain/Events/StockReservationExpiredDomainEvent.cs`
- `Products.Application/EventHandlers/LowStockWarningEventHandler.cs`
- `Products.Application/EventHandlers/StockReservationExpiredEventHandler.cs`

**Domain Events**:
```csharp
LowStockWarningDomainEvent(Guid ProductId, int AvailableQuantity, int Threshold) : DomainEventBase
StockReservationExpiredDomainEvent(Guid OrderId, Guid ProductId, int Quantity) : DomainEventBase
```

**Event Raising**:
| Event | Raised In | Condition |
|-------|-----------|-----------|
| LowStockWarningDomainEvent | Stock.ReserveStock() | AvailableQuantity <= LowStockThreshold |
| StockReservationExpiredDomainEvent | Stock.ExpireStaleReservations() | For each expired reservation |

**Design Decision**:
- Domain events are internal to bounded context
- StockReserved/StockReleased events not implemented (not needed for current requirements)
- Integration events for cross-service communication will be added in Phase 7 (Messaging)

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 8: Published Events)

---
## Notes
- TODO in StockReservationExpiredEventHandler: Publish integration event to Order service to cancel the order
