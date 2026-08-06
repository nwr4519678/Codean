namespace Platform.Domain.Primitives;

/// <summary>
/// Marker interface for domain events.
/// Pure — zero framework or infrastructure dependencies.
/// MediatR wiring lives exclusively in Infrastructure/Messaging/EventDispatcher.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique event identifier.</summary>
    Guid Id { get; }

    /// <summary>UTC timestamp of when the event occurred.</summary>
    DateTime OccurredOn { get; }
}
