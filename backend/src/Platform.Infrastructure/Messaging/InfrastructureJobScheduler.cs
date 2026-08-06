using Hangfire;
using Platform.Infrastructure.Jobs;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// Provides Hangfire recurring job schedules for Infrastructure jobs.
/// Called from Platform.Api so the Api doesn't need to reference internal job types directly.
/// This keeps the scheduling concern in the Api (which host to run) while the job
/// type resolution stays encapsulated inside Infrastructure.
/// </summary>
public static class InfrastructureJobScheduler
{
    /// <summary>
    /// Registers all Infrastructure recurring jobs with Hangfire.
    /// Safe to call on every startup — Hangfire uses the job key to upsert.
    /// </summary>
    public static void RegisterAll()
    {
        // Outbox processor — every 30 seconds
        RecurringJob.AddOrUpdate<ProcessOutboxJob>(
            "outbox-processor",
            job => job.ExecuteAsync(),
            "*/30 * * * * *");

        // Token & outbox archive cleanup — daily at 02:00 UTC
        RecurringJob.AddOrUpdate<TokenCleanupJob>(
            "token-cleanup",
            job => job.ExecuteAsync(),
            Cron.Daily(hour: 2));
    }
}
