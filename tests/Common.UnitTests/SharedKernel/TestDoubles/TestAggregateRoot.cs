using EShop.SharedKernel.Domain;
using EShop.SharedKernel.Events;

namespace EShop.Common.UnitTests.SharedKernel.TestDoubles;

/// <summary>
/// Concrete implementation of AggregateRoot for testing purposes.
/// </summary>
internal sealed class TestAggregateRoot : AggregateRoot
{
    public TestAggregateRoot(Guid id)
    {
        Id = id;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    public new void IncrementVersion()
    {
        base.IncrementVersion();
    }
}
