using EShop.SharedKernel.Events;

namespace EShop.Common.UnitTests.SharedKernel.TestDoubles;

/// <summary>
/// Concrete implementation of IDomainEvent for testing purposes.
/// </summary>
internal sealed record TestDomainEvent(string Message) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
