namespace Platform.Domain.Primitives;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Aggregate roots are the entry point for all state mutations and own domain event collection.
/// Entities that are not aggregate roots should NOT inherit this class.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Read-only snapshot of all domain events raised during the current operation.
    /// Events are dispatched and cleared by the Infrastructure OutboxInterceptor.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event and appends it to the uncommitted events collection.
    /// Call this from within aggregate methods to record that something meaningful occurred.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all raised domain events after they have been persisted to the outbox.
    /// Called exclusively by <c>OutboxInterceptor</c> after SaveChanges.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Returns and clears all domain events atomically.
    /// Useful for testing — lets you assert events without a separate Clear call.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList().AsReadOnly();
        _domainEvents.Clear();
        return events;
    }
}
