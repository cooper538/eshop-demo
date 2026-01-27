# Task 05: Product Domain Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | task-04 |

## Objective
Comprehensive unit tests for Product Service domain entities and business logic.

## Scope
- [ ] Test `ProductEntity`
  - [ ] Creation with valid parameters
  - [ ] Creation with invalid parameters (Guard failures)
  - [ ] Update methods (UpdateDetails, UpdatePrice)
  - [ ] IsLowStock calculation
- [ ] Test `StockEntity` (if separate from Product)
  - [ ] ReserveStock with sufficient quantity
  - [ ] ReserveStock with insufficient quantity
  - [ ] ReleaseStock
  - [ ] ConfirmReservation
  - [ ] Reservation expiration logic
- [ ] Test `StockReservation` value object
  - [ ] Creation and validation
  - [ ] Expiration check (IsExpired)
- [ ] Test `CategoryEntity`
  - [ ] Creation and validation
- [ ] Test command handlers
  - [ ] CreateProductCommandHandler
  - [ ] UpdateProductCommandHandler
  - [ ] ReserveStockCommandHandler
  - [ ] ReleaseStockCommandHandler
- [ ] Test validators
  - [ ] CreateProductCommandValidator (representative test)

## Dependencies
- Depends on: task-04
- Blocks: task-11

## Acceptance Criteria
- [ ] All domain entity state transitions tested
- [ ] Stock operations cover success and failure paths
- [ ] Command handlers tested with mocked dependencies

## Notes
- Use EF Core InMemory for handler tests or mock IProductDbContext
- Focus on business logic, not infrastructure
- Test domain events are raised correctly
