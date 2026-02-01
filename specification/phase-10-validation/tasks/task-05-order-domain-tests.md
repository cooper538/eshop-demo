# Task 05: Order Domain Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |

## Summary
Unit tests for Order.Domain - the core business logic with state machine, domain events, and value calculations.

## Project Setup
- [ ] Create `tests/Order.UnitTests/` project
- [ ] Reference Order.Domain, Order.Application projects
- [ ] Update solution

## Scope

### OrderEntity State Machine
- [ ] Test creation with valid data and initial status
- [ ] Test total amount calculation from items
- [ ] Test Confirm transition (Created -> Confirmed)
- [ ] Test Reject transition (Created -> Rejected)
- [ ] Test Cancel transition (Confirmed -> Cancelled)
- [ ] Test invalid transitions throw exception
- [ ] Test domain events raised on transitions

### OrderItem
- [ ] Test creation with valid data
- [ ] Test LineTotal calculation (Quantity x UnitPrice)
- [ ] Test validation (zero quantity, negative price)

### InvalidOrderStateException
- [ ] Test exception contains current and target status
- [ ] Test exception message is descriptive

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- OrderEntity state machine is the main showcase for DDD testing
- Test all valid transitions AND invalid transitions
- Domain events should contain all necessary data for handlers
