using EShop.Order.Domain.Enums;
using EShop.Order.Domain.Events;
using EShop.Order.Domain.Exceptions;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Domain.Entities;

public partial class OrderEntityTests
{
    [Fact]
    public void Cancel_WhenConfirmed_TransitionsToCancelled()
    {
        // Arrange
        var order = OrderTestHelper.CreateConfirmedOrder();
        order.ClearDomainEvents();
        var occurredAt = DateTime.UtcNow;
        const string reason = "Customer request";

        // Act
        order.Cancel(reason, occurredAt);

        // Assert
        order.Status.Should().Be(EOrderStatus.Cancelled);
        order.RejectionReason.Should().Be(reason);
        order.UpdatedAt.Should().Be(occurredAt);
    }

    [Fact]
    public void Cancel_IncrementsVersion()
    {
        // Arrange
        var order = OrderTestHelper.CreateConfirmedOrder();
        var initialVersion = order.Version;

        // Act
        order.Cancel("Reason", DateTime.UtcNow);

        // Assert
        order.Version.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void Cancel_RaisesOrderCancelledDomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        const string customerEmail = "cancel@example.com";
        var order = OrderTestHelper.CreateConfirmedOrder(customerId, customerEmail);
        order.ClearDomainEvents();
        var occurredAt = DateTime.UtcNow;
        const string reason = "Customer changed mind";

        // Act
        order.Cancel(reason, occurredAt);

        // Assert
        order
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<OrderCancelledDomainEvent>()
            .Which.Should()
            .BeEquivalentTo(
                new
                {
                    OrderId = order.Id,
                    CustomerId = customerId,
                    CustomerEmail = customerEmail,
                    Reason = reason,
                    OccurredOn = occurredAt,
                }
            );
    }

    [Theory]
    [InlineData(EOrderStatus.Created)]
    [InlineData(EOrderStatus.Rejected)]
    [InlineData(EOrderStatus.Cancelled)]
    public void Cancel_WhenNotConfirmed_ThrowsException(EOrderStatus status)
    {
        // Arrange
        var order = OrderTestHelper.CreateOrderWithStatus(status);

        // Act
        var act = () => order.Cancel("Reason", DateTime.UtcNow);

        // Assert
        act.Should()
            .Throw<InvalidOrderStateException>()
            .Where(ex => ex.CurrentStatus == status && ex.TargetStatus == EOrderStatus.Cancelled);
    }
}
