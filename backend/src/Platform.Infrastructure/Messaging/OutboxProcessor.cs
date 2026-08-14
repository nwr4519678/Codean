using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// Processes pending OutboxMessages by deserializing domain events
/// and dispatching them via the internal EventDispatcher.
///
/// SRP: This class owns the processing logic only.
/// Scheduling (when to run) is the responsibility of ProcessOutboxJob.
/// </summary>
internal sealed class OutboxProcessor(
    AppDbContext dbContext,
    EventDispatcher dispatcher,
    ILogger<OutboxProcessor> logger)
{
    private const int BatchSize = 20;
    private const int MaxRetries = 5;
    private static readonly TimeSpan ClaimDuration = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Processes up to <see cref="BatchSize"/> unprocessed outbox messages.
    /// Messages that have failed more than <see cref="MaxRetries"/> times are skipped.
    /// </summary>
    public async Task ProcessAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var workerId = $"{Environment.MachineName}:{Environment.ProcessId}";
        var messages = await dbContext.Set<Persistence.Entities.OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries &&
                        (m.ClaimedUntil == null || m.ClaimedUntil < now))
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            message.ClaimedBy = workerId;
            message.ClaimedUntil = now.Add(ClaimDuration);
        }
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("OutboxProcessor: processing {Count} message(s)", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message.Type, message.Payload, ct);
                message.ProcessedAt = DateTime.UtcNow;
                message.Error       = null;
                message.ClaimedBy = null;
                message.ClaimedUntil = null;

                logger.LogDebug("Outbox message processed: Id={Id}, Type={Type}", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = $"[Attempt {message.RetryCount}] {ex.Message}";
                message.ClaimedBy = null;
                message.ClaimedUntil = null;

                logger.LogError(ex,
                    "Outbox message failed (attempt {Attempt}/{Max}): Id={Id}, Type={Type}",
                    message.RetryCount, MaxRetries, message.Id, message.Type);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
