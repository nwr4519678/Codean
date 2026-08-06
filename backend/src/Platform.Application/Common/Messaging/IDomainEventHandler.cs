using MediatR;
using Platform.Domain.Primitives;

namespace Platform.Application.Common.Messaging;

/// <summary>
/// Typed base interface for domain event handlers in the Application layer.
/// Handlers implement this to process a specific <typeparamref name="TEvent"/>.
///
/// Usage:
///   public sealed class UserRegisteredEventHandler
///       : IDomainEventHandler&lt;UserRegisteredEvent&gt;
///   {
///       public Task Handle(UserRegisteredEvent e, CancellationToken ct) { ... }
///   }
///
/// Wire-up is done by Infrastructure's EventDispatcher which wraps IDomainEvent
/// → INotification and publishes via IPublisher — MediatR never touches Domain.
/// </summary>
/// <typeparam name="TEvent">The domain event type this handler processes.</typeparam>
public interface IDomainEventHandler<TEvent> : INotificationHandler<DomainEventWrapper<TEvent>>
    where TEvent : IDomainEvent;

/// <summary>
/// Internal MediatR notification wrapper used by the EventDispatcher.
/// Keeps MediatR types out of both Domain and Infrastructure/Messaging public APIs.
/// This record is defined in Application.Common so handlers can receive it without
/// depending on Infrastructure.
/// </summary>
public sealed record DomainEventWrapper<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
