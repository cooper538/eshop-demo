namespace EShop.Order.UnitTests;

public class PlaceholderTests
{
    private readonly Mock<IDisposable> _mockDisposable = new();

    [Theory]
    [AutoData]
    public void Placeholder_ShouldPass(int value)
    {
        // TODO: Replace with actual Order domain tests
        value.Should().BeGreaterThan(int.MinValue);
        _mockDisposable.Object.Should().NotBeNull();
    }
}
