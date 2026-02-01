using EShop.Order.Domain.Enums;
using EShop.Order.UnitTests.Helpers;

namespace EShop.Order.UnitTests.Domain.Entities;

public partial class OrderEntityTests
{
    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        const string customerEmail = "customer@example.com";
        var createdAt = DateTime.UtcNow;
        var items = new[] { OrderTestHelper.CreateOrderItem(unitPrice: 50.00m, quantity: 2) };

        // Act
        var order = OrderTestHelper.CreateOrder(customerId, customerEmail, items, createdAt);

        // Assert
        order
            .Should()
            .BeEquivalentTo(
                new
                {
                    CustomerId = customerId,
                    CustomerEmail = customerEmail,
                    Status = EOrderStatus.Created,
                    CreatedAt = createdAt,
                    UpdatedAt = (DateTime?)null,
                    RejectionReason = (string?)null,
                }
            );
        order.Id.Should().NotBeEmpty();
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Create_CalculatesTotalAmountFromItems()
    {
        // Arrange
        var items = new[]
        {
            OrderTestHelper.CreateOrderItem(unitPrice: 10.00m, quantity: 2),
            OrderTestHelper.CreateOrderItem(unitPrice: 15.00m, quantity: 3),
        };

        // Act
        var order = OrderTestHelper.CreateOrder(items: items);

        // Assert
        order.TotalAmount.Should().Be(65.00m); // (10*2) + (15*3) = 20 + 45
    }
}
