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
- [x] Create ReserveStockCommand record with OrderId and Items
- [x] Create OrderItemDto record (ProductId, Quantity)
- [x] Implement ReserveStockCommandHandler
- [x] Add idempotency check (existing reservation for same OrderId returns success)
- [x] Load Stock aggregates with active reservations
- [x] Delegate reservation creation to Stock.ReserveStock()
- [x] Create StockReservationResult with success/failure factory methods
- [x] Create ReleaseStockCommand record with OrderId
- [x] Implement ReleaseStockCommandHandler
- [x] Find stocks with active reservations by OrderId
- [x] Delegate release to Stock.ReleaseReservation()
- [x] Create StockReleaseResult with success/failure factory methods

## Implementation Details

**Files**:
- `Products.Application/Commands/ReserveStock/ReserveStockCommand.cs`
- `Products.Application/Commands/ReserveStock/ReserveStockCommandHandler.cs`
- `Products.Application/Commands/ReleaseStock/ReleaseStockCommand.cs`
- `Products.Application/Commands/ReleaseStock/ReleaseStockCommandHandler.cs`
- `Products.Application/Dtos/OrderItemDto.cs`
- `Products.Application/Dtos/StockReservationResult.cs`
- `Products.Application/Dtos/StockReleaseResult.cs`

**Command Signatures**:
```csharp
ReserveStockCommand(Guid OrderId, IReadOnlyList<OrderItemDto> Items) : ICommand<StockReservationResult>
ReleaseStockCommand(Guid OrderId) : ICommand<StockReleaseResult>
```

**Dependencies**:
- IProductDbContext - database access
- IDateTimeProvider - testable timestamps
- IStockReservationOptions - configurable reservation duration

**Idempotency**:
- ReserveStock: If reservation exists for OrderId, return success (Active) or AlreadyProcessed (Released/Expired)
- ReleaseStock: If no active reservations found, return success (idempotent)

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 4: Stock Reservation Flow, Section 7: Idempotency)

---
## Notes
(Updated during implementation)
