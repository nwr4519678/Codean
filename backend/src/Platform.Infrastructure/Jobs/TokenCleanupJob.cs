using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence.Context;
using Platform.Infrastructure.Persistence.Entities;

namespace Platform.Infrastructure.Jobs;

/// <summary>
/// Hangfire daily job that purges:
/// 1. Expired refresh tokens older than 7 days past expiry.
/// 2. Processed outbox messages older than 30 days (archive cleanup).
/// </summary>
public sealed class TokenCleanupJob(
    AppDbContext dbContext,
    ILogger<TokenCleanupJob> logger)
{
    private static readonly TimeSpan TokenRetentionPeriod  = TimeSpan.FromDays(7);
    private static readonly TimeSpan OutboxRetentionPeriod = TimeSpan.FromDays(30);

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;

        // 1. Delete expired refresh tokens
        var tokenCutoff = now - TokenRetentionPeriod;
        var deletedTokens = await dbContext.Set<RefreshToken>()
            .Where(t => t.ExpiresAt < tokenCutoff)
            .ExecuteDeleteAsync();

        if (deletedTokens > 0)
            logger.LogInformation("TokenCleanup: deleted {Count} expired refresh token(s)", deletedTokens);

        // 2. Delete processed outbox messages older than retention period
        var outboxCutoff = now - OutboxRetentionPeriod;
        var deletedOutbox = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < outboxCutoff)
            .ExecuteDeleteAsync();

        if (deletedOutbox > 0)
            logger.LogInformation("TokenCleanup: purged {Count} old outbox message(s)", deletedOutbox);
    }
}
