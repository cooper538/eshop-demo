# Task 05: CQRS Commands

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03 |

## Summary
Implement command handlers for product CRUD and stock management operations.

## Scope
- [x] Create CreateProductCommand with Name, Description, Price, StockQuantity, LowStockThreshold, Category
- [x] Create CreateProductCommandHandler (creates both Product and Stock entities)
- [x] Create CreateProductCommandMapper (extension method ToEntity)
- [x] Return created ProductDto with generated Id
- [x] Create UpdateProductCommand with Id, Name, Description, Price, LowStockThreshold, Category
- [x] Create UpdateProductCommandHandler
- [x] Throw NotFoundException when product not found
- [x] Handle optimistic concurrency conflicts (ConflictException)
- [x] Create ReserveStockCommand(OrderId, Items) for stock reservation
- [x] Create ReserveStockCommandHandler with transactional reservation
- [x] Create ReleaseStockCommand(OrderId) for releasing reservations
- [x] Create ReleaseStockCommandHandler
- [x] Create ExpireReservationsCommand(BatchSize) for background job
- [x] Create ExpireReservationsCommandHandler

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.2: Create Product)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 3: Exception Hierarchy)

---
## Notes
**Command structure**:
```
Commands/
├── CreateProduct/
│   ├── CreateProductCommand.cs (includes mapper)
│   ├── CreateProductCommandHandler.cs
│   └── CreateProductCommandValidator.cs
├── UpdateProduct/
│   ├── UpdateProductCommand.cs
│   ├── UpdateProductCommandHandler.cs
│   └── UpdateProductCommandValidator.cs
├── ReserveStock/
│   ├── ReserveStockCommand.cs
│   ├── ReserveStockCommandHandler.cs
│   └── ReserveStockCommandValidator.cs
├── ReleaseStock/
│   ├── ReleaseStockCommand.cs
│   └── ReleaseStockCommandHandler.cs
└── ExpireReservations/
    ├── ExpireReservationsCommand.cs
    └── ExpireReservationsCommandHandler.cs
```

**Stock reservation flow**:
1. ReserveStock - reserves stock for all order items atomically
2. ReleaseStock - releases all reservations for an order
3. ExpireReservations - background job expires stale reservations

**Result DTOs**:
- `StockReservationResult(IsSuccess, FailureMessage)`
- `StockReleaseResult(Success, FailureReason)`
