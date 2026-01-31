using EShop.Common.Application.Correlation;

namespace EShop.Common.UnitTests.Application.Correlation;

public class CorrelationContextTests
{
    [Fact]
    public void CreateScope_SetsCurrentContext_AccessibleInScope()
    {
        // Arrange
        var correlationId = "test-correlation-123";

        // Act
        using (CorrelationContext.CreateScope(correlationId))
        {
            // Assert
            CorrelationContext.Current.Should().NotBeNull();
            CorrelationContext.Current!.CorrelationId.Should().Be(correlationId);
        }
    }

    [Fact]
    public void CreateScope_Nested_RestoresPreviousContext()
    {
        // Arrange
        var level1Id = "level1";
        var level2Id = "level2";
        var level3Id = "level3";

        // Act & Assert - 3 levels of nesting
        using (CorrelationContext.CreateScope(level1Id))
        {
            CorrelationContext.Current!.CorrelationId.Should().Be(level1Id);

            using (CorrelationContext.CreateScope(level2Id))
            {
                CorrelationContext.Current!.CorrelationId.Should().Be(level2Id);

                using (CorrelationContext.CreateScope(level3Id))
                {
                    CorrelationContext.Current!.CorrelationId.Should().Be(level3Id);
                }

                CorrelationContext.Current!.CorrelationId.Should().Be(level2Id);
            }

            CorrelationContext.Current!.CorrelationId.Should().Be(level1Id);
        }

        // After all scopes disposed, should be null
        CorrelationContext.Current.Should().BeNull();
    }
}
