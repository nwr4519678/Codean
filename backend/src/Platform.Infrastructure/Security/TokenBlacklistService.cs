using System;
using System.Threading;
using System.Threading.Tasks;
using Platform.Application.Common.Abstractions;

namespace Platform.Infrastructure.Security;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string jti, TimeSpan timeToLive, CancellationToken ct = default);
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);
}

public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheService _cache;

    public TokenBlacklistService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan timeToLive, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti)) return;
        var cacheKey = $"blacklist:jti:{jti}";
        await _cache.SetAsync(cacheKey, true, timeToLive, ct: ct);
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti)) return false;
        var cacheKey = $"blacklist:jti:{jti}";
        var isBlacklisted = await _cache.GetAsync<bool?>(cacheKey, ct);
        return isBlacklisted.HasValue && isBlacklisted.Value;
    }
}
