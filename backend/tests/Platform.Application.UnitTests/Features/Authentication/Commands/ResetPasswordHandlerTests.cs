using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Features.Authentication.Commands.ResetPassword;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class ResetPasswordHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IRepository<RefreshToken> _refreshTokens = Substitute.For<IRepository<RefreshToken>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ResetPasswordHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public ResetPasswordHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.IpAddress.Returns("127.0.0.1");

        _sut = new ResetPasswordHandler(
            _users,
            _auditLogs,
            _refreshTokens,
            _uow,
            _hasher,
            _current,
            _clock);
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnResetInvalidError()
    {
        // Arrange
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var cmd = new ResetPasswordCommand("user@example.com", "token", "NewPassword123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.reset_invalid");
    }

    [Fact]
    public async Task Handle_WhenResetLogNotFound_ShouldReturnResetInvalidError()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@example.com", IsActive = true };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        _auditLogs.FirstOrDefaultAsync(Arg.Any<Expression<Func<AuditLog, bool>>>(), Arg.Any<CancellationToken>())
                  .Returns((AuditLog?)null);

        var cmd = new ResetPasswordCommand("user@example.com", "token", "NewPassword123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.reset_invalid");
    }

    [Fact]
    public async Task Handle_WhenResetLogIsExpired_ShouldReturnResetExpiredError()
    {
        // Arrange
        var rawToken = "my_token";
        var tokenHash = HashToken(rawToken);

        var user = new User { Id = 1, Email = "user@example.com", IsActive = true };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        var resetLog = new AuditLog
        {
            UserId = 1,
            Action = "User.PasswordResetRequested",
            CreatedAt = _now.UtcDateTime.AddMinutes(-31),
            NewValues = $"{{\"TokenHash\":\"{tokenHash}\"}}"
        };
        _auditLogs.FirstOrDefaultAsync(Arg.Any<Expression<Func<AuditLog, bool>>>(), Arg.Any<CancellationToken>())
                  .Returns(resetLog);

        var cmd = new ResetPasswordCommand("user@example.com", rawToken, "NewPassword123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.reset_expired");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPasswordConsumeLogRevokeTokensAndReturnSuccess()
    {
        // Arrange
        var rawToken = "valid_token";
        var tokenHash = HashToken(rawToken);

        var user = new User { Id = 1, Email = "user@example.com", IsActive = true, PasswordHash = "old_hash" };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        var resetLog = new AuditLog
        {
            UserId = 1,
            Action = "User.PasswordResetRequested",
            CreatedAt = _now.UtcDateTime.AddMinutes(-10),
            NewValues = $"{{\"TokenHash\":\"{tokenHash}\"}}"
        };
        _auditLogs.FirstOrDefaultAsync(Arg.Any<Expression<Func<AuditLog, bool>>>(), Arg.Any<CancellationToken>())
                  .Returns(resetLog);

        _hasher.Hash("NewPassword123!").Returns("new_hashed_password");

        var token1 = new RefreshToken { Id = 10, UserId = 1, RevokedAt = null };
        _refreshTokens.ListAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(new List<RefreshToken> { token1 });

        var cmd = new ResetPasswordCommand("user@example.com", rawToken, "NewPassword123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hashed_password");
        resetLog.NewValues.Should().Contain("CONSUMED");
        token1.RevokedAt.Should().Be(_now.UtcDateTime);
        token1.ReplacedByToken.Should().Be("PASSWORD_RESET");

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
