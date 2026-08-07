using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Features.Authentication.Commands.ChangePassword;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class ChangePasswordHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<RefreshToken> _refreshTokens = Substitute.For<IRepository<RefreshToken>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ChangePasswordHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public ChangePasswordHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.UserId.Returns(10L);
        _current.IpAddress.Returns("127.0.0.1");

        _sut = new ChangePasswordHandler(
            _users,
            _refreshTokens,
            _auditLogs,
            _uow,
            _hasher,
            _cache,
            _current,
            _clock);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _current.UserId.Returns((long?)null);
        var cmd = new ChangePasswordCommand("OldPass123!", "NewPass123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.unauthenticated");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _users.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns((User?)null);
        var cmd = new ChangePasswordCommand("OldPass123!", "NewPass123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.user_not_found");
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIncorrect_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = new User { Id = 10, PasswordHash = "hash" };
        _users.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("WrongPass!", "hash").Returns(false);

        var cmd = new ChangePasswordCommand("WrongPass!", "NewPass123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.wrong_password");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldChangePasswordRevokeTokensAndInvalidateCache()
    {
        // Arrange
        var user = new User { Id = 10, PasswordHash = "old_hash" };
        _users.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("OldPass123!", "old_hash").Returns(true);
        _hasher.Hash("NewPass123!").Returns("new_hash");

        var token1 = new RefreshToken { Id = 1, UserId = 10, RevokedAt = null };
        var token2 = new RefreshToken { Id = 2, UserId = 10, RevokedAt = null };
        _refreshTokens.ListAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(new List<RefreshToken> { token1, token2 });

        var cmd = new ChangePasswordCommand("OldPass123!", "NewPass123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hash");
        user.UpdatedAt.Should().Be(_now.UtcDateTime);

        token1.RevokedAt.Should().Be(_now.UtcDateTime);
        token1.ReplacedByToken.Should().Be("PASSWORD_CHANGED");
        token2.RevokedAt.Should().Be(_now.UtcDateTime);

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cache.Received(2).RemoveAsync(Arg.Is<string>(k => k.Contains("10")), Arg.Any<CancellationToken>());
    }
}
