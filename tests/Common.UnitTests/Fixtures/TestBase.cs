using AutoFixture.AutoMoq;

namespace EShop.Common.UnitTests.Fixtures;

/// <summary>
/// Base class for unit tests providing common setup and utilities.
/// Inherit from this class when you need more control over fixture configuration.
/// </summary>
/// <remarks>
/// For most tests, prefer using [Theory, AutoMoqData] attribute instead.
/// Use TestBase when you need:
/// - Custom fixture configuration per test class
/// - Shared test data across multiple tests
/// - Complex setup that can't be done via attributes
/// </remarks>
public abstract class TestBase : IDisposable
{
    protected IFixture Fixture { get; }

    protected TestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });

        // Handle recursive types
        Fixture
            .Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Allow derived classes to customize
        ConfigureFixture(Fixture);
    }

    /// <summary>
    /// Override to add custom fixture configuration.
    /// </summary>
    protected virtual void ConfigureFixture(IFixture fixture) { }

    /// <summary>
    /// Creates an instance of T with all dependencies auto-generated.
    /// </summary>
    protected T Create<T>() => Fixture.Create<T>();

    /// <summary>
    /// Creates multiple instances of T.
    /// </summary>
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count);

    /// <summary>
    /// Freezes a value in the fixture - the same instance will be returned
    /// for all subsequent Create calls for this type.
    /// </summary>
    protected T Freeze<T>() => Fixture.Freeze<T>();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
