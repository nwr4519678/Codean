using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Settings;
using Platform.Application.Features.Authentication.Commands.Login;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class LoginHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<Role> _roles = Substitute.For<IRepository<Role>>();
    private readonly IRepository<RefreshToken> _refreshTokens = Substitute.For<IRepository<RefreshToken>>();
    private readonly IRepository<UserSession> _userSessions = Substitute.For<IRepository<UserSession>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenIssuer _tokenIssuer = Substitute.For<ITokenIssuer>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly LockoutSettings _lockoutSettings = new() { MaxFailedAttempts = 3, LockoutDuration = TimeSpan.FromMinutes(15) };
    private readonly LoginHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public LoginHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.IpAddress.Returns("127.0.0.1");
        _current.UserAgent.Returns("TestBrowser");

        var options = Options.Create(_lockoutSettings);

        _sut = new LoginHandler(
            _users,
            _roles,
            _refreshTokens,
            _userSessions,
            _auditLogs,
            _uow,
            _hasher,
            _tokenIssuer,
            _current,
            _clock,
            options);
    }

    [Fact]
    public async Task Handle_WhenUserIsLockedOut_ShouldReturnForbidden()
    {
        // Arrange
        var user = new User
        {
            Email = "locked@example.com",
            LockoutEnd = _now.UtcDateTime.AddMinutes(10)
        };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        var cmd = new LoginCommand("locked@example.com", "Password123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.locked_out");
        _hasher.DidNotReceiveWithAnyArgs().Verify(default!, default!);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var cmd = new LoginCommand("nonexistent@example.com", "Password123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.invalid_credentials");
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldIncrementFailedLoginCountAndReturnUnauthorized()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@example.com", PasswordHash = "hash", FailedLoginCount = 1 };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);
        _hasher.Verify("WrongPass!", "hash").Returns(false);

        var cmd = new LoginCommand("user@example.com", "WrongPass!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.invalid_credentials");
        user.FailedLoginCount.Should().Be(2);
        user.LockoutEnd.Should().BeNull();
        _users.Received(1).Update(user);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFailedAttemptsExceedMax_ShouldSetLockoutEnd()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@example.com", PasswordHash = "hash", FailedLoginCount = 2 };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);
        _hasher.Verify("WrongPass!", "hash").Returns(false);

        var cmd = new LoginCommand("user@example.com", "WrongPass!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        user.FailedLoginCount.Should().Be(0); // reset counter after lockout triggers
        user.LockoutEnd.Should().Be(_now.UtcDateTime.AddMinutes(15));
    }

    [Fact]
    public async Task Handle_WhenAccountDisabled_ShouldReturnForbidden()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@example.com", PasswordHash = "hash", IsActive = false };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);
        _hasher.Verify("Pass123!", "hash").Returns(true);

        var cmd = new LoginCommand("user@example.com", "Pass123!");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.account_disabled");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldIssueTokensAndReturnLoginResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 10,
            Email = "student@example.com",
            FullName = "Student User",
            PasswordHash = "hash",
            RoleId = 1,
            IsActive = true,
            FailedLoginCount = 2,
            LockoutEnd = _now.UtcDateTime.AddMinutes(-5)
        };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);
        _hasher.Verify("CorrectPass!", "hash").Returns(true);

        var issuedTokens = new IssuedTokens("access_token", _now.AddMinutes(15), "refresh_token", _now.AddDays(7), "test-jti");
        _tokenIssuer.Issue(Arg.Any<AccessTokenDescriptor>(), "TestBrowser", "127.0.0.1")
                    .Returns(issuedTokens);

        var cmd = new LoginCommand("student@example.com", "CorrectPass!", RememberMe: false);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.Role.Should().Be("Student");

        user.FailedLoginCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLogin.Should().Be(_now.UtcDateTime);

        await _refreshTokens.Received(1).AddAsync(Arg.Is<RefreshToken>(r =>
            r.User == user &&
            r.ExpiresAt == issuedTokens.RefreshTokenExpiresAt.UtcDateTime
        ), Arg.Any<CancellationToken>());

        await _userSessions.Received(1).AddAsync(Arg.Is<UserSession>(s =>
            s.User == user &&
            s.DeviceInfo == "TestBrowser" &&
            s.IpAddress == "127.0.0.1" &&
            s.IsActive == true
        ), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
