# Task 05: Order Domain Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | task-01 |
| Est. Tests | ~18 |

## Summary
Unit tests for Order.Domain - the core business logic with state machine, domain events, and value calculations.

## Project Setup
- [ ] Create `tests/Order.UnitTests/` project
- [ ] Reference Order.Domain, Order.Application projects
- [ ] Create folder structure: `Domain/`, `Application/`
- [ ] Update `EShopDemo.sln`

## Scope

### OrderEntity State Machine (~10 tests)

**Creation:**
- [ ] `Create_WithValidData_ReturnsOrderWithCreatedStatus` - initial state is Created
- [ ] `Create_WithItems_CalculatesTotalAmount` - TotalAmount = sum of LineTotal
- [ ] `Create_RaisesDomainEvent` - OrderCreatedDomainEvent (if exists)

**Confirm Transition:**
- [ ] `Confirm_WhenCreated_ChangesStatusToConfirmed` - Created → Confirmed
- [ ] `Confirm_WhenCreated_RaisesOrderConfirmedDomainEvent` - event with correct data
- [ ] `Confirm_WhenNotCreated_ThrowsInvalidOrderStateException` - invalid transition

**Reject Transition:**
- [ ] `Reject_WhenCreated_ChangesStatusToRejected` - Created → Rejected
- [ ] `Reject_WhenCreated_RaisesOrderRejectedDomainEvent` - event with reason
- [ ] `Reject_WhenNotCreated_ThrowsInvalidOrderStateException` - invalid transition

**Cancel Transition:**
- [ ] `Cancel_WhenConfirmed_ChangesStatusToCancelled` - Confirmed → Cancelled
- [ ] `Cancel_WhenConfirmed_RaisesOrderCancelledDomainEvent` - event with reason
- [ ] `Cancel_WhenNotConfirmed_ThrowsInvalidOrderStateException` - invalid transition

### OrderItem (~4 tests)
- [ ] `Create_WithValidData_ReturnsOrderItem` - factory method works
- [ ] `LineTotal_CalculatesCorrectly` - Quantity × UnitPrice
- [ ] `Create_WithZeroQuantity_ThrowsException` - validation
- [ ] `Create_WithNegativePrice_ThrowsException` - validation

### InvalidOrderStateException (~2 tests)
- [ ] `Exception_ContainsCurrentAndTargetStatus` - properties set correctly
- [ ] `Exception_MessageDescribesTransition` - readable message

## Test Implementation Notes

```csharp
// Example: State transition test
[Fact]
public void Confirm_WhenCreated_ChangesStatusToConfirmed()
{
    // Arrange
    var order = OrderEntity.Create(
        customerId: Guid.NewGuid(),
        customerEmail: "test@example.com",
        items: new[] { CreateTestItem() }
    );

    // Act
    order.Confirm();

    // Assert
    order.Status.Should().Be(EOrderStatus.Confirmed);
}

// Example: Domain event test
[Fact]
public void Confirm_WhenCreated_RaisesOrderConfirmedDomainEvent()
{
    // Arrange
    var order = OrderEntity.Create(...);

    // Act
    order.Confirm();

    // Assert
    order.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<OrderConfirmedDomainEvent>()
        .Which.OrderId.Should().Be(order.Id);
}

// Example: Invalid transition test
[Fact]
public void Confirm_WhenAlreadyConfirmed_ThrowsInvalidOrderStateException()
{
    // Arrange
    var order = OrderEntity.Create(...);
    order.Confirm();

    // Act
    var act = () => order.Confirm();

    // Assert
    act.Should().Throw<InvalidOrderStateException>()
       .Where(e => e.CurrentStatus == EOrderStatus.Confirmed);
}
```

## Related Specs
- [unit-testing.md](../../high-level-specs/unit-testing.md)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- OrderEntity state machine is the main showcase for DDD testing
- Test all valid transitions AND invalid transitions (throw exceptions)
- Domain events should contain all necessary data for handlers
- OrderItem is an owned entity (not aggregate) - simpler tests
