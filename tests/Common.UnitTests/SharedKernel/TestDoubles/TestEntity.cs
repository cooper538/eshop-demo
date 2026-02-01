using EShop.SharedKernel.Domain;

namespace EShop.Common.UnitTests.SharedKernel.TestDoubles;

/// <summary>
/// Concrete implementation of Entity for testing purposes.
/// </summary>
internal sealed class TestEntity : Entity
{
    public TestEntity(Guid id)
    {
        Id = id;
    }
}
