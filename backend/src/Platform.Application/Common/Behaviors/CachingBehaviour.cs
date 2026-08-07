using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Caching;
using Platform.Application.Common.Abstractions;

namespace Platform.Application.Common.Behaviors;

public sealed class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableRequest
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(ICacheService cache, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.BypassCache)
        {
            _logger.LogInformation("Bypassing cache for request {RequestName} ({CacheKey})", typeof(TRequest).Name, request.CacheKey);
            return await next();
        }

        var cachedResult = await _cache.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache hit for request {RequestName} ({CacheKey})", typeof(TRequest).Name, request.CacheKey);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for request {RequestName} ({CacheKey}). Fetching fresh data...", typeof(TRequest).Name, request.CacheKey);
        var response = await next();

        if (response is not null)
        {
            var tags = request.Tags.Count > 0 ? request.Tags : null;
            await _cache.SetAsync(request.CacheKey, response, request.Expiration, tags, cancellationToken);
        }

        return response;
    }
}
