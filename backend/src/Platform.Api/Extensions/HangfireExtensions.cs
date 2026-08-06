using Hangfire;
using Microsoft.AspNetCore.Builder;
using Platform.Api.Authorization;
using Platform.Infrastructure.Messaging;

namespace Platform.Api.Extensions;

/// <summary>
/// Registers the Hangfire dashboard and all recurring background jobs.
/// Scheduling concerns belong here — job implementations live in Platform.Infrastructure/Jobs.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Mounts the Hangfire dashboard at <c>/jobs</c> restricted to authenticated Admin users.
    /// </summary>
    public static WebApplication MapPlatformHangfire(this WebApplication app)
    {
        app.UseHangfireDashboard("/jobs", new DashboardOptions
        {
            Authorization        = [new HangfireAdminFilter()],
            DashboardTitle       = "Platform — Background Jobs",
            StatsPollingInterval = 5000
        });

        return app;
    }

    /// <summary>
    /// Registers all platform recurring jobs via the Infrastructure job scheduler facade.
    /// Platform.Api decides when to run; Infrastructure owns the job implementations.
    /// </summary>
    public static WebApplication RegisterPlatformJobs(this WebApplication app)
    {
        InfrastructureJobScheduler.RegisterAll();
        return app;
    }
}
