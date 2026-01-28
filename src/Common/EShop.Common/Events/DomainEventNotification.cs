using EShop.SharedKernel.Events;
using MediatR;

namespace EShop.Common.Events;

// Adapts domain events to MediatR notifications
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
