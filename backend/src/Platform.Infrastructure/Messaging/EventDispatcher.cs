using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Messaging;
using Platform.Domain.Primitives;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// Internal adapter that bridges IDomainEvent → INotification for MediatR dispatch.
///
/// This class is intentionally sealed and internal — it contains the only MediatR
/// coupling in the entire Messaging subsystem. The DomainEventWrapper type is defined
/// in Application.Common.Messaging so Application event handlers can receive it
/// without depending on Infrastructure.
///
/// Called exclusively by OutboxProcessor — never exposed outside this assembly.
/// </summary>
internal sealed class EventDispatcher(
    IPublisher publisher,
    ILogger<EventDispatcher> logger)
{
    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Deserializes a raw outbox payload → IDomainEvent and publishes it via MediatR.
    /// </summary>
    public async Task DispatchAsync(string typeName, string payload, CancellationToken ct)
    {
        var eventType = Type.GetType(typeName);
        if (eventType is null)
        {
            logger.LogWarning("Cannot resolve domain event type: {TypeName}", typeName);
            return;
        }

        var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(payload, eventType, DeserializerOptions);
        if (domainEvent is null)
        {
            logger.LogWarning("Deserialization returned null for type {TypeName}", typeName);
            return;
        }

        await DispatchAsync(domainEvent, ct);
    }

    /// <summary>
    /// Wraps an IDomainEvent in a DomainEventWrapper and publishes via IPublisher.
    /// Uses reflection to create the correct generic wrapper type.
    /// </summary>
    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        // Create DomainEventWrapper<TEvent> for the specific event type
        var wrapperType  = typeof(DomainEventWrapper<>).MakeGenericType(domainEvent.GetType());
        var notification = Activator.CreateInstance(wrapperType, domainEvent) as INotification;

        if (notification is null)
        {
            logger.LogError("Could not create DomainEventWrapper for {EventType}", domainEvent.GetType().Name);
            return;
        }

        logger.LogDebug("Dispatching domain event {EventType} (Id={EventId})",
            domainEvent.GetType().Name, domainEvent.Id);

        await publisher.Publish(notification, ct);
    }
}
