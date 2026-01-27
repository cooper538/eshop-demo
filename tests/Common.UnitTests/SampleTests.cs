namespace EShop.Common.UnitTests;

/// <summary>
/// Sample tests demonstrating testing patterns.
/// These tests verify the test infrastructure works correctly.
/// </summary>
public class SampleTests
{
    [Fact]
    public void TestInfrastructure_WhenRunningTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Addition_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Act
        var result = a + b;

        // Assert
        result.Should().Be(expected);
    }

    [Theory, AutoMoqData]
    public void AutoMoqData_WhenUsed_GeneratesTestData(string randomString, int randomInt)
    {
        // Assert - AutoFixture generates non-null/non-default values
        randomString.Should().NotBeNullOrEmpty();
        randomInt.Should().NotBe(0); // Very unlikely to be 0
    }

    [Theory, AutoMoqData]
    public void AutoMoqData_WithFrozenMock_UsesSameInstance(
        [Frozen] Mock<ISampleDependency> mockDependency,
        SampleService sut
    )
    {
        // Arrange
        mockDependency.Setup(x => x.GetValue()).Returns(42);

        // Act
        var result = sut.GetDependencyValue();

        // Assert
        result.Should().Be(42);
        mockDependency.Verify(x => x.GetValue(), Times.Once);
    }
}

// Sample types for demonstrating AutoMoq
public interface ISampleDependency
{
    int GetValue();
}

public class SampleService(ISampleDependency dependency)
{
    public int GetDependencyValue() => dependency.GetValue();
}
