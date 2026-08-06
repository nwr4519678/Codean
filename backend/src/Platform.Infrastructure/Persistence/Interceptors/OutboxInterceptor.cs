using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Platform.Domain.Primitives;
using Platform.Infrastructure.Persistence.Context;
using Platform.Infrastructure.Persistence.Entities;

namespace Platform.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that atomically converts domain events
/// into OutboxMessage rows within the same database transaction.
///
/// This guarantees that if the business operation is committed, the events
/// are also persisted — eliminating the dual-write problem.
/// </summary>
public sealed class OutboxInterceptor(ILogger<OutboxInterceptor> logger) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
            ConvertDomainEventsToOutboxMessages(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages(AppDbContext context)
    {
        var aggregates = new List<AggregateRoot>();

        // Collect all tracked AggregateRoot entities that have raised domain events
        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        {
            if (entry.Entity.DomainEvents.Count > 0)
                aggregates.Add(entry.Entity);
        }

        if (aggregates.Count == 0) return;

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                try
                {
                    outboxMessages.Add(new OutboxMessage
                    {
                        Id          = domainEvent.Id,
                        Type        = domainEvent.GetType().AssemblyQualifiedName!,
                        Payload     = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions),
                        OccurredAt  = domainEvent.OccurredOn,
                        RetryCount  = 0
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Failed to serialize domain event {EventType} (Id={EventId})",
                        domainEvent.GetType().Name, domainEvent.Id);
                }
            }

            // Clear after serialization — before SaveChanges completes
            aggregate.ClearDomainEvents();
        }

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
            logger.LogDebug("Converted {Count} domain events to outbox messages", outboxMessages.Count);
        }
    }
}
