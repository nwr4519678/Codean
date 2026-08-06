namespace Platform.Application.Common.Caching;

public interface ICacheableRequest
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
    bool BypassCache => false;
}
