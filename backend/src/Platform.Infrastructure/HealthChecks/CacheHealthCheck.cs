using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Platform.Infrastructure.HealthChecks;

public sealed class CacheHealthCheck(HybridCache cache) : IHealthCheck
{
    private const string ProbeKey = "healthcheck:cache:probe";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Write a probe value and read it back — validates both write and read paths
            var probeValue = Guid.NewGuid().ToString();

            await cache.SetAsync(ProbeKey, probeValue,
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromSeconds(10) },
                cancellationToken: cancellationToken);

            var retrieved = await cache.GetOrCreateAsync<string>(
                ProbeKey, _ => ValueTask.FromResult(probeValue),
                cancellationToken: cancellationToken);

            return retrieved is not null
                ? HealthCheckResult.Healthy("Cache is operational.")
                : HealthCheckResult.Degraded("Cache write/read round-trip returned null.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cache health check failed.", ex);
        }
    }
}
