using EShop.Common.Application.Correlation;

namespace EShop.Common.UnitTests.Application.Correlation;

public class CorrelationIdAccessorTests
{
    [Fact]
    public void GetCorrelationId_WithContext_ReturnsContextId()
    {
        // Arrange
        var expectedId = "context-correlation-id";
        var accessor = new CorrelationIdAccessor();

        // Act
        string actualId;
        using (CorrelationContext.CreateScope(expectedId))
        {
            actualId = accessor.CorrelationId;
        }

        // Assert
        actualId.Should().Be(expectedId);
    }

    [Fact]
    public void GetCorrelationId_NoContext_ReturnsNewGuid()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act
        var correlationId = accessor.CorrelationId;

        // Assert
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("should return a valid GUID");
    }
}
