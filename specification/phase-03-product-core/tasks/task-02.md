# Task 02: Domain Model

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement domain entities with separated stock management and reservation tracking.

## Scope
- [x] Create ProductEntity (AggregateRoot) with Name, Description, Price, Category, CreatedAt, UpdatedAt
- [x] Create StockEntity (AggregateRoot) with ProductId, Quantity, LowStockThreshold, Reservations
- [x] Create StockReservationEntity (Entity) with OrderId, ProductId, Quantity, ReservedAt, ExpiresAt, Status
- [x] Create EReservationStatus enum (Active, Released, Expired)
- [x] Implement StockEntity.ReserveStock() returning StockReservationEntity
- [x] Implement StockEntity.ReleaseReservation(orderId)
- [x] Implement StockEntity.ExpireStaleReservations(now)
- [x] Implement StockEntity.AvailableQuantity computed property
- [x] Implement StockEntity.IsLowStock computed property
- [x] Create domain events (ProductCreated, ProductUpdated, LowStockWarning, StockReservationExpired)
- [x] Create InsufficientStockException in Domain/Exceptions

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 5: Domain Model)

---
## Notes
**Design decision**: Stock is separated from Product entity for better domain modeling:
- `ProductEntity` - catalog information (name, description, price, category)
- `StockEntity` - inventory management (quantity, reservations, thresholds)
- `StockReservationEntity` - tracks active reservations with expiration

**Domain Events**:
- `ProductCreatedDomainEvent(ProductId, InitialStockQuantity, LowStockThreshold)`
- `ProductUpdatedDomainEvent(ProductId, LowStockThreshold)`
- `LowStockWarningDomainEvent(ProductId, AvailableQuantity, LowStockThreshold)`
- `StockReservationExpiredDomainEvent(OrderId, ProductId, Quantity)`

**Reservation flow**:
1. Order requests stock reservation via gRPC
2. StockEntity creates reservation with expiration time
3. On order completion, reservation is released
4. Background job expires stale reservations
