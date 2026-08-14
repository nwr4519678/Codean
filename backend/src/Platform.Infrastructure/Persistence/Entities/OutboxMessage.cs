using System;

namespace Platform.Infrastructure.Persistence.Entities;

/// <summary>
/// Infrastructure persistence entity for the Transactional Outbox Pattern.
/// Written in the same DB transaction as the business operation —
/// guarantees at-least-once delivery of domain events to external systems.
/// NOT a domain entity — lives exclusively in Infrastructure.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Unique event identifier (same as IDomainEvent.Id).</summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Fully-qualified CLR type name of the domain event.
    /// Used by EventDispatcher to resolve the correct type during deserialization.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>JSON-serialized domain event payload.</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>UTC timestamp of when the domain event occurred.</summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>UTC timestamp of when this message was successfully processed. Null = pending.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Last error message if processing failed. Null = no failure.</summary>
    public string? Error { get; set; }

    /// <summary>Number of processing attempts made so far. Capped at 5.</summary>
    public int RetryCount { get; set; }

    /// <summary>Short lease that prevents concurrent workers from processing the same message.</summary>
    public DateTime? ClaimedUntil { get; set; }

    /// <summary>Worker instance that currently owns the lease.</summary>
    public string? ClaimedBy { get; set; }
}
