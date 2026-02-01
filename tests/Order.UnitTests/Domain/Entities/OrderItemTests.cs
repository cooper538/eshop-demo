using EShop.Order.Domain.Entities;

namespace EShop.Order.UnitTests.Domain.Entities;

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string productName = "Test Product";
        const int quantity = 5;
        const decimal unitPrice = 19.99m;

        // Act
        var item = OrderItem.Create(productId, productName, quantity, unitPrice);

        // Assert
        item.Should()
            .BeEquivalentTo(
                new
                {
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                }
            );
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(5, 19.99, 99.95)]
    [InlineData(3, 33.33, 99.99)]
    [InlineData(0, 100.00, 0.00)]
    public void LineTotal_CalculatesCorrectly(
        int quantity,
        decimal unitPrice,
        decimal expectedTotal
    )
    {
        // Arrange
        var item = OrderItem.Create(Guid.NewGuid(), "Product", quantity, unitPrice);

        // Act
        var lineTotal = item.LineTotal;

        // Assert
        lineTotal.Should().Be(expectedTotal);
    }
}
