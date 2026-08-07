using Platform.Application.Common.Abstractions;

namespace Platform.Application.Common.Caching;

public interface ICacheableRequest
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
    bool BypassCache => false;

    /// <summary>
    /// Tags applied when storing the cache entry.
    /// Use <see cref="ICacheService.RemoveByTagAsync"/> to bulk-invalidate all entries sharing a tag.
    /// </summary>
    IReadOnlyList<string> Tags => [];
}
