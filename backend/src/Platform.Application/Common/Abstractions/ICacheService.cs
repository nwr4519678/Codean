namespace Platform.Application.Common.Abstractions;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByTagAsync(string tag, CancellationToken ct = default);
}
