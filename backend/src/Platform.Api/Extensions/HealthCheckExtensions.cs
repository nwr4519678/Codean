using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Platform.Api.Middleware;

namespace Platform.Api.Extensions;

/// <summary>
/// Registers health check endpoints on the application pipeline.
/// Endpoint mapping is the Api's responsibility — implementations are registered in Infrastructure DI.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Maps the three standard health check endpoints:
    /// <list type="bullet">
    ///   <item><c>/health/live</c>  — liveness probe (always 200 if process is up)</item>
    ///   <item><c>/health/ready</c> — readiness probe (all "ready" tagged checks)</item>
    ///   <item><c>/health</c>       — full JSON report of all checks</item>
    /// </list>
    /// </summary>
    public static WebApplication MapPlatformHealthChecks(this WebApplication app)
    {
        // Liveness — just confirms the process is alive
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = static (ctx, _) =>
            {
                ctx.Response.ContentType = "text/plain";
                return ctx.Response.WriteAsync("Alive");
            }
        });

        // Readiness — only checks tagged "ready" (DB, cache, SMTP)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate      = r => r.Tags.Contains("ready"),
            ResponseWriter = WriteJsonReport
        });

        // Full report — all registered checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteJsonReport
        });

        app.MapGet("/metrics", () => Results.Text(ApiMetrics.Snapshot(), "text/plain; version=0.0.4"));

        return app;
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    private static Task WriteJsonReport(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status     = report.Status.ToString(),
            duration   = report.TotalDuration.TotalMilliseconds,
            components = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status      = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration    = e.Value.Duration.TotalMilliseconds,
                    error       = e.Value.Status == HealthStatus.Unhealthy ? "unhealthy" : null
                })
        }, JsonOpts);

        return ctx.Response.WriteAsync(result);
    }
}
