using EShop.Order.Domain.Enums;
using EShop.Order.Domain.Events;
using EShop.Order.Domain.Exceptions;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Domain.Entities;

public partial class OrderEntityTests
{
    [Fact]
    public void Confirm_WhenCreated_TransitionsToConfirmed()
    {
        // Arrange
        var order = OrderTestHelper.CreateOrder();
        var occurredAt = DateTime.UtcNow;

        // Act
        order.Confirm(occurredAt);

        // Assert
        order.Status.Should().Be(EOrderStatus.Confirmed);
        order.UpdatedAt.Should().Be(occurredAt);
    }

    [Fact]
    public void Confirm_IncrementsVersion()
    {
        // Arrange
        var order = OrderTestHelper.CreateOrder();
        var initialVersion = order.Version;

        // Act
        order.Confirm(DateTime.UtcNow);

        // Assert
        order.Version.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void Confirm_RaisesOrderConfirmedDomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        const string customerEmail = "test@example.com";
        var items = new[]
        {
            OrderTestHelper.CreateOrderItem(
                productName: "Product 1",
                quantity: 2,
                unitPrice: 10.00m
            ),
        };
        var order = OrderTestHelper.CreateOrder(customerId, customerEmail, items);
        var occurredAt = DateTime.UtcNow;

        // Act
        order.Confirm(occurredAt);

        // Assert
        order
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<OrderConfirmedDomainEvent>()
            .Which.Should()
            .BeEquivalentTo(
                new
                {
                    OrderId = order.Id,
                    CustomerId = customerId,
                    CustomerEmail = customerEmail,
                    TotalAmount = 20.00m,
                    OccurredOn = occurredAt,
                }
            );
        order.DomainEvents.OfType<OrderConfirmedDomainEvent>().Single().Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(EOrderStatus.Confirmed)]
    [InlineData(EOrderStatus.Rejected)]
    [InlineData(EOrderStatus.Cancelled)]
    public void Confirm_WhenNotCreated_ThrowsException(EOrderStatus status)
    {
        // Arrange
        var order = OrderTestHelper.CreateOrderWithStatus(status);

        // Act
        var act = () => order.Confirm(DateTime.UtcNow);

        // Assert
        act.Should()
            .Throw<InvalidOrderStateException>()
            .Where(ex => ex.CurrentStatus == status && ex.TargetStatus == EOrderStatus.Confirmed);
    }
}
