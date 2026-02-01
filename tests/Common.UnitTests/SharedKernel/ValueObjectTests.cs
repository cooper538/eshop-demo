using EShop.Common.UnitTests.SharedKernel.TestDoubles;

namespace EShop.Common.UnitTests.SharedKernel;

public class ValueObjectTests
{
    [Fact]
    public void Equals_WithSameComponents_ReturnsTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        vo1.Should().Be(vo2);
    }

    [Fact]
    public void Equals_WithDifferentComponents_ReturnsFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("different", 42);

        // Act & Assert
        vo1.Should().NotBe(vo2);
    }

    [Fact]
    public void EqualityOperator_WithSameComponents_ReturnsTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        (vo1 == vo2)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentComponents_ReturnsTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("different", 42);

        // Act & Assert
        (vo1 != vo2)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameComponents_ReturnsSameHash()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }
}
