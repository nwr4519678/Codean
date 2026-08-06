using Microsoft.Extensions.Caching.Hybrid;
using Platform.Application.Common.Abstractions;

namespace Platform.Infrastructure.Caching.Hybrid;

/// <summary>
/// .NET 10 Hybrid Cache implementation — L1 in-process + L2 Redis.
/// Provides automatic stampede protection and serialization.
/// </summary>
public sealed class HybridCacheService : ICacheService
{
    private readonly HybridCache _cache;

    public HybridCacheService(HybridCache cache) => _cache = cache;

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        var options = ttl.HasValue
            ? new HybridCacheEntryOptions { Expiration = ttl }
            : null;

        return await _cache.GetOrCreateAsync(key, factory, options, cancellationToken: ct);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        // HybridCache does not expose a raw Get — use GetOrCreateAsync with null factory
        // Return default if not found by using a sentinel
        T? result = default;
        await _cache.GetOrCreateAsync<T?>(
            key,
            _ => ValueTask.FromResult<T?>(default),
            new HybridCacheEntryOptions { Expiration = TimeSpan.Zero },
            cancellationToken: ct);
        return result;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var options = ttl.HasValue
            ? new HybridCacheEntryOptions { Expiration = ttl }
            : null;

        return _cache.SetAsync(key, value, options, cancellationToken: ct).AsTask();
    }

    public Task RemoveAsync(string key, CancellationToken ct = default) =>
        _cache.RemoveAsync(key, ct).AsTask();

    public Task RemoveByTagAsync(string tag, CancellationToken ct = default) =>
        _cache.RemoveByTagAsync(tag, ct).AsTask();
}
