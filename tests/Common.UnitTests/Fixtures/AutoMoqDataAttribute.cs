using AutoFixture.AutoMoq;

namespace EShop.Common.UnitTests.Fixtures;

/// <summary>
/// [Theory, AutoMoqData] - auto-generates test data and mocks for interfaces.
/// Use [Frozen] to reuse same mock instance across parameters.
/// </summary>
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(CreateFixture) { }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize(
            new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true }
        );

        // Handle circular references
        fixture
            .Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}

/// <summary>
/// [InlineAutoMoqData("value")] - explicit inline values + auto-generated rest.
/// </summary>
public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AutoMoqDataAttribute(), values) { }
}
