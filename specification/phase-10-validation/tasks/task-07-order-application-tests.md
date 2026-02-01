# Task 07: Order Application Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-01, task-05 |

## Summary
Unit tests for Order.Application - command handlers, query handlers, and validators.

## Scope

### CreateOrderCommandValidator
- [x] Test valid command passes
- [x] Test required fields (CustomerId, Email, Items)
- [x] Test email format validation

### CreateOrderItemDtoValidator
- [x] Test valid item passes
- [x] Test quantity and price validation

### CancelOrderCommandValidator
- [x] Test valid command passes
- [x] Test required OrderId
- [x] Test reason max length

### CreateOrderCommandHandler
- [x] Test happy path (stock available -> order confirmed)
- [x] Test stock unavailable scenario
- [x] Test Product Service failure propagation
- [x] Test order persistence

### CancelOrderCommandHandler
- [x] Test happy path (cancel confirmed order)
- [x] Test order not found scenario
- [x] Test invalid state scenario
- [x] Test best-effort stock release (doesn't fail on release error)

### Query Handlers
- [x] Test GetOrderByIdQueryHandler (found, not found)
- [x] Test GetOrdersQueryHandler (pagination, filtering)

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- Uses EF Core InMemory for handler tests (pragmatic approach)
- Mock: IProductServiceClient, IDateTimeProvider
- Validators use TheoryData + MemberData pattern
