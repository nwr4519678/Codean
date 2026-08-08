using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Caching;
using Platform.Application.Common.Abstractions;
using Platform.Domain.Results;

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

        // Result<T> is a readonly struct with private constructors — JSON cannot
        // round-trip it. Detect this case and cache the inner value type instead.
        var resultInnerType = GetResultInnerType(typeof(TResponse));

        if (resultInnerType is not null)
        {
            // Try to get the cached inner value
            var cachedInner = await GetCachedInnerAsync(resultInnerType, request.CacheKey, cancellationToken);
            if (cachedInner is not null)
            {
                _logger.LogInformation("Cache hit for request {RequestName} ({CacheKey})", typeof(TRequest).Name, request.CacheKey);
                return WrapInResult(resultInnerType, cachedInner);
            }

            _logger.LogInformation("Cache miss for request {RequestName} ({CacheKey}). Fetching fresh data...", typeof(TRequest).Name, request.CacheKey);
            var response = await next();

            // Only cache successful results — extract the inner value
            var innerValue = ExtractInnerValue(resultInnerType, response);
            if (innerValue is not null)
            {
                var tags = request.Tags.Count > 0 ? request.Tags : null;
                await SetCachedInnerAsync(resultInnerType, request.CacheKey, innerValue, request.Expiration, tags, cancellationToken);
            }

            return response;
        }

        // Non-Result responses: original behaviour
        var cached = await _cache.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for request {RequestName} ({CacheKey})", typeof(TRequest).Name, request.CacheKey);
            return cached;
        }

        _logger.LogInformation("Cache miss for request {RequestName} ({CacheKey}). Fetching fresh data...", typeof(TRequest).Name, request.CacheKey);
        var plainResponse = await next();

        if (plainResponse is not null)
        {
            var tags = request.Tags.Count > 0 ? request.Tags : null;
            await _cache.SetAsync(request.CacheKey, plainResponse, request.Expiration, tags, cancellationToken);
        }

        return plainResponse;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns the T in Result&lt;T&gt;, or null if TResponse is not Result&lt;T&gt;.</summary>
    private static Type? GetResultInnerType(Type type)
    {
        if (!type.IsGenericType) return null;
        var def = type.GetGenericTypeDefinition();
        if (def == typeof(Result<>)) return type.GetGenericArguments()[0];
        return null;
    }

    private async Task<object?> GetCachedInnerAsync(Type innerType, string key, CancellationToken ct)
    {
        // Call _cache.GetAsync<innerType>(key, ct) via reflection
        var method = typeof(ICacheService)
            .GetMethod(nameof(ICacheService.GetAsync))!
            .MakeGenericMethod(innerType);
        var task = (Task)method.Invoke(_cache, [key, ct])!;
        await task.ConfigureAwait(false);
        return ((dynamic)task).Result;
    }

    private static object? ExtractInnerValue(Type innerType, TResponse response)
    {
        // Result<T>.IsSuccess and Result<T>.Value via reflection
        var type = typeof(Result<>).MakeGenericType(innerType);
        var isSuccess = (bool)type.GetProperty(nameof(Result<object>.IsSuccess))!.GetValue(response)!;
        if (!isSuccess) return null;
        return type.GetProperty(nameof(Result<object>.Value))!.GetValue(response);
    }

    private static TResponse WrapInResult(Type innerType, object innerValue)
    {
        // Result<T>.Success(value)
        var type = typeof(Result<>).MakeGenericType(innerType);
        var successMethod = type.GetMethod("Success", BindingFlags.Public | BindingFlags.Static, [innerType])!;
        return (TResponse)successMethod.Invoke(null, [innerValue])!;
    }

    private async Task SetCachedInnerAsync(Type innerType, string key, object value, TimeSpan? ttl, IReadOnlyList<string>? tags, CancellationToken ct)
    {
        var method = typeof(ICacheService)
            .GetMethod(nameof(ICacheService.SetAsync))!
            .MakeGenericMethod(innerType);
        var task = (Task)method.Invoke(_cache, [key, value, ttl, tags, ct])!;
        await task.ConfigureAwait(false);
    }
}
