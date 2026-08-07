using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Commands.RevokeSession;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class RevokeSessionHandlerTests
{
    private readonly IRepository<UserSession> _sessions = Substitute.For<IRepository<UserSession>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RevokeSessionHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public RevokeSessionHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.UserId.Returns(10L);
        _current.IpAddress.Returns("127.0.0.1");

        _sut = new RevokeSessionHandler(
            _sessions,
            _auditLogs,
            _uow,
            _cache,
            _current,
            _clock);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _current.UserId.Returns((long?)null);
        var cmd = new RevokeSessionCommand(100L);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.unauthenticated");
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _sessions.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns((UserSession?)null);
        var cmd = new RevokeSessionCommand(100L);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.session_not_found");
    }

    [Fact]
    public async Task Handle_WhenSessionBelongsToAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        var session = new UserSession { Id = 100L, UserId = 999L, IsActive = true };
        _sessions.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns(session);
        var cmd = new RevokeSessionCommand(100L);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.session_not_found");
    }

    [Fact]
    public async Task Handle_WhenSessionAlreadyInactive_ShouldReturnConflict()
    {
        // Arrange
        var session = new UserSession { Id = 100L, UserId = 10L, IsActive = false };
        _sessions.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns(session);
        var cmd = new RevokeSessionCommand(100L);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.session_already_revoked");
    }

    [Fact]
    public async Task Handle_WithValidActiveSession_ShouldDeactivateSessionSaveAndInvalidateCache()
    {
        // Arrange
        var session = new UserSession { Id = 100L, UserId = 10L, IsActive = true };
        _sessions.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns(session);
        var cmd = new RevokeSessionCommand(100L);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.IsActive.Should().BeFalse();
        session.LogoutAt.Should().Be(_now.UtcDateTime);

        _sessions.Received(1).Update(session);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(Arg.Is<string>(k => k.Contains("10")), Arg.Any<CancellationToken>());
    }
}
