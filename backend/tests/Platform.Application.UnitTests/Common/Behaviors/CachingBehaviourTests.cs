using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Behaviors;
using Platform.Application.Common.Caching;
using Xunit;

namespace Platform.Application.UnitTests.Common.Behaviors;

public class CachingBehaviourTests
{
    public record CacheableTestRequest(string Key, bool Bypass = false) : ICacheableRequest, IRequest<string>
    {
        public string CacheKey => Key;
        public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
        public bool BypassCache => Bypass;
    }

    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<CachingBehaviour<CacheableTestRequest, string>> _logger = Substitute.For<ILogger<CachingBehaviour<CacheableTestRequest, string>>>();

    [Fact]
    public async Task Handle_WhenBypassCacheIsTrue_ShouldCallNextAndSkipCacheLookup()
    {
        // Arrange
        var behavior = new CachingBehaviour<CacheableTestRequest, string>(_cache, _logger);
        var request = new CacheableTestRequest("test_key", Bypass: true);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("fresh_value"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("fresh_value");
        await _cache.DidNotReceiveWithAnyArgs().GetAsync<string>(default!, default!);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedValueWithoutCallingNext()
    {
        // Arrange
        _cache.GetAsync<string>("test_key", Arg.Any<CancellationToken>())
              .Returns("cached_value");

        var behavior = new CachingBehaviour<CacheableTestRequest, string>(_cache, _logger);
        var request = new CacheableTestRequest("test_key");
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("fresh_value"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.Should().Be("cached_value");
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldCallNextAndStoreResultInCache()
    {
        // Arrange
        _cache.GetAsync<string>("test_key", Arg.Any<CancellationToken>())
              .Returns((string?)null);

        var behavior = new CachingBehaviour<CacheableTestRequest, string>(_cache, _logger);
        var request = new CacheableTestRequest("test_key");
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("fresh_value"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("fresh_value");
        await _cache.Received(1).SetAsync("test_key", "fresh_value", request.Expiration, tags: null, ct: Arg.Any<CancellationToken>());
    }
}
