# Task 03: Order Unit Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Create Order.UnitTests project with comprehensive tests for Order domain entities, command handlers, and business logic - the main showcase for unit testing skills.

## Scope

### Project Setup
- [ ] Create `tests/Order.UnitTests/` project
- [ ] Reference Order.Domain, Order.Application projects
- [ ] Create folder structure: `Domain/`, `Application/Commands/`, `Application/Validators/`, `Helpers/`
- [ ] Add `TestDbContextFactory` helper for InMemory EF Core
- [ ] Update `EShopDemo.sln`

### Domain Tests (Critical - State Machine)
- [ ] Test `OrderEntity` state transitions
  - [ ] Create() - valid order creation with items
  - [ ] Confirm() - Created → Confirmed (happy path)
  - [ ] Reject() - Created → Rejected with reason
  - [ ] Cancel() - Confirmed → Cancelled with reason
  - [ ] Invalid transitions throw `InvalidOrderStateException`
- [ ] Test `OrderItem` value object
  - [ ] Creation with valid parameters
  - [ ] LineTotal calculation (quantity × unitPrice)

### Command Handler Tests
- [ ] Test `CreateOrderCommandHandler`
  - [ ] Stock available → Order confirmed
  - [ ] Stock unavailable → Order rejected
  - [ ] Product service failure → proper error handling
- [ ] Test `CancelOrderCommandHandler`
  - [ ] Confirmed order → Cancelled + stock released
  - [ ] Already cancelled order → error
  - [ ] Non-existent order → NotFoundException

### Validator Tests (Representative)
- [ ] Test `CreateOrderCommandValidator`
  - [ ] Valid command passes
  - [ ] Empty items fails
  - [ ] Invalid customer data fails

## Related Specs
- → [unit-testing.md](../../high-level-specs/unit-testing.md) (Section: Mocking Strategy)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: Order Lifecycle)

---
## Notes
- This is the **main showcase** for unit testing - be thorough
- Use Moq for IProductServiceClient, ILogger
- Use InMemory EF Core DbContext for handler tests
- Follow naming: `MethodName_Scenario_ExpectedResult`
- Order state machine is critical business logic - cover all paths
