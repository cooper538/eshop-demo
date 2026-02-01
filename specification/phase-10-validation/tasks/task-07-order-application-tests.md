# Task 07: Order Application Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | 🔵 in_progress |
| Dependencies | task-01, task-05 |

## Summary
Unit tests for Order.Application - command handlers, query handlers, and validators.

## Scope

### CreateOrderCommandValidator
- [ ] Test valid command passes
- [ ] Test required fields (CustomerId, Email, Items)
- [ ] Test email format validation

### CreateOrderItemDtoValidator
- [ ] Test valid item passes
- [ ] Test quantity and price validation

### CancelOrderCommandValidator
- [ ] Test valid command passes
- [ ] Test required OrderId
- [ ] Test reason max length

### CreateOrderCommandHandler
- [ ] Test happy path (stock available -> order confirmed)
- [ ] Test stock unavailable scenario
- [ ] Test Product Service failure propagation
- [ ] Test order persistence

### CancelOrderCommandHandler
- [ ] Test happy path (cancel confirmed order)
- [ ] Test order not found scenario
- [ ] Test invalid state scenario
- [ ] Test best-effort stock release (doesn't fail on release error)

### Query Handlers
- [ ] Test GetOrderByIdQueryHandler (found, not found)
- [ ] Test GetOrdersQueryHandler (pagination, filtering)

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- Use AutoFixture + AutoMoq for automatic mocking
- Mock: IOrderDbContext, IProductServiceClient, IDateTimeProvider
