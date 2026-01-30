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
  - [ ] Create() - valid order creation with items, raises `OrderCreatedDomainEvent`
  - [ ] Confirm() - Created → Confirmed, raises `OrderConfirmedDomainEvent`
  - [ ] Reject() - Created → Rejected with reason, raises `OrderRejectedDomainEvent`
  - [ ] Cancel() - Confirmed → Cancelled with reason, raises `OrderCancelledDomainEvent`
  - [ ] Invalid transitions throw `InvalidOrderStateException`
- [ ] Test `OrderItem` (owned entity, not ValueObject)
  - [ ] Creation with valid parameters
  - [ ] LineTotal calculation (quantity × unitPrice)
  - [ ] Note: No equality tests needed - OrderItem is IOwnedEntity

### Command Handler Tests
- [ ] Test `CreateOrderCommandHandler`
  - [ ] Stock available → Order created and confirmed
  - [ ] Stock unavailable → throws `InvalidOperationException` (reservation fails)
  - [ ] Product service failure → exception propagates
- [ ] Test `CancelOrderCommandHandler`
  - [ ] Confirmed order → Cancelled + stock release attempted
  - [ ] Non-Confirmed order → returns error Result (Success: false)
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
- **Implementation note**: CreateOrderCommandHandler throws on stock failure (doesn't create rejected order)
