using EShop.SharedKernel.Guards;

namespace EShop.Common.UnitTests.SharedKernel;

public class GuardTests
{
    [Fact]
    public void AgainstNull_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Guard.Against.Null(value, nameof(value));

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AgainstNullOrEmpty_WhenNullOrEmpty_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => Guard.Against.NullOrEmpty(value, nameof(value));

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgainstNegativeOrZero_WhenNotPositive_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act
        var act = () => Guard.Against.NegativeOrZero(value, nameof(value));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(value));
    }

    [Fact]
    public void AgainstNegative_WhenNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1m;

        // Act
        var act = () => Guard.Against.Negative(value, nameof(value));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(value));
    }
}
