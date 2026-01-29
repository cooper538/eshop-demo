using System.Collections.Concurrent;
using System.Reflection;
using EShop.SharedKernel.Events;
using MediatR;

namespace EShop.Common.Application.Events;

public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new();

    private readonly IPublisher _publisher;

    public MediatRDomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var eventType = domainEvent.GetType();
        var constructor = ConstructorCache.GetOrAdd(eventType, CreateConstructor);
        var notification = constructor.Invoke([domainEvent]);

        await _publisher.Publish(notification, cancellationToken);
    }

    private static ConstructorInfo CreateConstructor(Type eventType)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
        return notificationType.GetConstructor([eventType])!;
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
