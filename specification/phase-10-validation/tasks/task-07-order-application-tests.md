# Task 07: Order Application Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ⚪ pending |
| Dependencies | task-01, task-05 |
| Est. Tests | ~22 |

## Summary
Unit tests for Order.Application - command handlers, query handlers, and validators.

## Scope

### CreateOrderCommandValidator (~4 tests)
- [ ] `Validate_ValidCommand_Passes` - valid command passes validation
- [ ] `Validate_EmptyCustomerId_Fails` - CustomerId required
- [ ] `Validate_InvalidEmail_Fails` - email format validation
- [ ] `Validate_EmptyItems_Fails` - at least one item required

### CreateOrderItemDtoValidator (~3 tests)
- [ ] `Validate_ValidItem_Passes` - valid item passes
- [ ] `Validate_ZeroQuantity_Fails` - quantity > 0
- [ ] `Validate_NegativePrice_Fails` - price >= 0

### CancelOrderCommandValidator (~3 tests)
- [ ] `Validate_ValidCommand_Passes` - valid command passes
- [ ] `Validate_EmptyOrderId_Fails` - OrderId required
- [ ] `Validate_ReasonTooLong_Fails` - max 500 chars

### CreateOrderCommandHandler (~4 tests)
- [ ] `Handle_StockAvailable_CreatesAndConfirmsOrder` - happy path
- [ ] `Handle_StockUnavailable_ThrowsInvalidOperationException` - stock reservation fails
- [ ] `Handle_ProductServiceFailure_PropagatesException` - gRPC failure
- [ ] `Handle_ValidOrder_PersistsToDatabase` - order saved

### CancelOrderCommandHandler (~4 tests)
- [ ] `Handle_ConfirmedOrder_CancelsSuccessfully` - happy path
- [ ] `Handle_OrderNotFound_ThrowsNotFoundException` - 404 case
- [ ] `Handle_InvalidState_ReturnsFailureResult` - Success: false
- [ ] `Handle_StockReleaseFailure_StillCancelsOrder` - best-effort release

### GetOrderByIdQueryHandler (~2 tests)
- [ ] `Handle_ExistingOrder_ReturnsOrderDto` - found case
- [ ] `Handle_NonExistentOrder_ThrowsNotFoundException` - 404 case

### GetOrdersQueryHandler (~2 tests)
- [ ] `Handle_WithPagination_ReturnsCorrectPage` - pagination works
- [ ] `Handle_FilterByCustomerId_ReturnsOnlyMatching` - filter works

## Test Implementation Notes

```csharp
// Example: Handler test with mocked dependencies
public class CreateOrderCommandHandlerTests : TestBase
{
    private readonly Mock<IOrderDbContext> _dbContextMock;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _dbContextMock = Freeze<Mock<IOrderDbContext>>();
        _productClientMock = Freeze<Mock<IProductServiceClient>>();
        _handler = Create<CreateOrderCommandHandler>();
    }

    [Fact]
    public async Task Handle_StockAvailable_CreatesAndConfirmsOrder()
    {
        // Arrange
        var command = Create<CreateOrderCommand>();
        _productClientMock
            .Setup(x => x.ReserveStockAsync(It.IsAny<...>()))
            .ReturnsAsync(new StockReservationResult { Success = true });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(EOrderStatus.Confirmed);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// Example: Validator test
[Fact]
public void Validate_EmptyItems_Fails()
{
    // Arrange
    var command = new CreateOrderCommand(
        CustomerId: Guid.NewGuid(),
        CustomerEmail: "test@example.com",
        Items: Array.Empty<CreateOrderItemDto>()
    );
    var validator = new CreateOrderCommandValidator();

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Items");
}
```

## Related Specs
- [unit-testing.md](../../high-level-specs/unit-testing.md)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- Use AutoFixture + AutoMoq for automatic mocking (TestBase from task-01)
- Handlers need mocked: IOrderDbContext, IProductServiceClient, IDateTimeProvider
- Validators use FluentValidation - test both valid and invalid cases
- CancelOrderCommandHandler has best-effort stock release (doesn't fail on release error)
