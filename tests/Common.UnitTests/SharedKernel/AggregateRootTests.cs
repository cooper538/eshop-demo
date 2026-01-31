using EShop.Common.UnitTests.SharedKernel.TestDoubles;

namespace EShop.Common.UnitTests.SharedKernel;

public class AggregateRootTests
{
    [Fact]
    public void AggregateRoot_AddDomainEvent_AddsToCollection()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestDomainEvent("Test event");

        // Act
        aggregate.RaiseDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AggregateRoot_AddMultipleEvents_AllPresent()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new TestDomainEvent("Event 2");
        var event3 = new TestDomainEvent("Event 3");

        // Act
        aggregate.RaiseDomainEvent(event1);
        aggregate.RaiseDomainEvent(event2);
        aggregate.RaiseDomainEvent(event3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().Contain(new[] { event1, event2, event3 });
    }

    [Fact]
    public void AggregateRoot_ClearDomainEvents_RemovesAll()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.RaiseDomainEvent(new TestDomainEvent("Event 1"));
        aggregate.RaiseDomainEvent(new TestDomainEvent("Event 2"));

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_Version_StartsAtZero()
    {
        // Arrange & Act
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        // Assert
        aggregate.Version.Should().Be(0);
    }

    [Fact]
    public void AggregateRoot_IncrementVersion_IncreasesByOne()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        // Act
        aggregate.IncrementVersion();
        aggregate.IncrementVersion();

        // Assert
        aggregate.Version.Should().Be(2);
    }
}
