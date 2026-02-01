using EShop.Order.Domain.Enums;
using EShop.Order.Domain.Events;
using EShop.Order.Domain.Exceptions;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Domain.Entities;

public partial class OrderEntityTests
{
    [Fact]
    public void Reject_WhenCreated_TransitionsToRejected()
    {
        // Arrange
        var order = OrderTestHelper.CreateOrder();
        var occurredAt = DateTime.UtcNow;
        const string reason = "Out of stock";

        // Act
        order.Reject(reason, occurredAt);

        // Assert
        order.Status.Should().Be(EOrderStatus.Rejected);
        order.RejectionReason.Should().Be(reason);
        order.UpdatedAt.Should().Be(occurredAt);
    }

    [Fact]
    public void Reject_IncrementsVersion()
    {
        // Arrange
        var order = OrderTestHelper.CreateOrder();
        var initialVersion = order.Version;

        // Act
        order.Reject("Reason", DateTime.UtcNow);

        // Assert
        order.Version.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void Reject_RaisesOrderRejectedDomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        const string customerEmail = "test@example.com";
        var order = OrderTestHelper.CreateOrder(customerId, customerEmail);
        var occurredAt = DateTime.UtcNow;
        const string reason = "Product unavailable";

        // Act
        order.Reject(reason, occurredAt);

        // Assert
        order
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<OrderRejectedDomainEvent>()
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
    [InlineData(EOrderStatus.Confirmed)]
    [InlineData(EOrderStatus.Rejected)]
    [InlineData(EOrderStatus.Cancelled)]
    public void Reject_WhenNotCreated_ThrowsException(EOrderStatus status)
    {
        // Arrange
        var order = OrderTestHelper.CreateOrderWithStatus(status);

        // Act
        var act = () => order.Reject("Reason", DateTime.UtcNow);

        // Assert
        act.Should()
            .Throw<InvalidOrderStateException>()
            .Where(ex => ex.CurrentStatus == status && ex.TargetStatus == EOrderStatus.Rejected);
    }
}
