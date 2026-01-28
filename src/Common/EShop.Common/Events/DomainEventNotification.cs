using EShop.SharedKernel.Events;
using MediatR;

namespace EShop.Common.Events;

/// <summary>
/// Wrapper that adapts domain events to MediatR notifications.
/// This allows SharedKernel to remain free of MediatR dependencies.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
