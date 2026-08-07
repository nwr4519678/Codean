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
using Platform.Application.Features.Authentication.Commands.RefreshToken;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class RefreshTokenHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<RefreshToken> _refreshTokens = Substitute.For<IRepository<RefreshToken>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ITokenIssuer _tokenIssuer = Substitute.For<ITokenIssuer>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RefreshTokenHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public RefreshTokenHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.IpAddress.Returns("127.0.0.1");
        _current.UserAgent.Returns("TestBrowser");

        _sut = new RefreshTokenHandler(
            _users,
            _refreshTokens,
            _auditLogs,
            _uow,
            _tokenIssuer,
            _current,
            _clock);
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns((RefreshToken?)null);

        var cmd = new RefreshTokenCommand("invalid_token");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.invalid_token");
    }

    [Fact]
    public async Task Handle_WhenTokenIsAlreadyRevoked_ShouldRevokeAllUserTokensAndReturnTokenReuseAlert()
    {
        // Arrange
        var rawToken = "used_token";
        var hash = HashToken(rawToken);
        var storedToken = new RefreshToken
        {
            UserId = 5,
            Token = hash,
            RevokedAt = _now.UtcDateTime.AddMinutes(-10)
        };

        _refreshTokens.FirstOrDefaultAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(storedToken);

        var active1 = new RefreshToken { Id = 1, UserId = 5, RevokedAt = null };
        var active2 = new RefreshToken { Id = 2, UserId = 5, RevokedAt = null };
        _refreshTokens.ListAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(new List<RefreshToken> { active1, active2 });

        var cmd = new RefreshTokenCommand(rawToken);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.token_reuse");

        active1.RevokedAt.Should().Be(_now.UtcDateTime);
        active1.ReplacedByToken.Should().Be("SECURITY_REVOKED");
        active2.RevokedAt.Should().Be(_now.UtcDateTime);

        await _auditLogs.Received(1).AddAsync(Arg.Is<AuditLog>(a =>
            a.Action == "Token.ReuseDetected" &&
            a.UserId == 5
        ), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ShouldReturnUnauthorized()
    {
        // Arrange
        var rawToken = "expired_token";
        var hash = HashToken(rawToken);
        var storedToken = new RefreshToken
        {
            UserId = 5,
            Token = hash,
            RevokedAt = null,
            ExpiresAt = _now.UtcDateTime.AddMinutes(-1)
        };

        _refreshTokens.FirstOrDefaultAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(storedToken);

        var cmd = new RefreshTokenCommand(rawToken);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.token_expired");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldRotateTokenAndReturnResponse()
    {
        // Arrange
        var rawToken = "valid_token";
        var hash = HashToken(rawToken);
        var storedToken = new RefreshToken
        {
            UserId = 5,
            Token = hash,
            RevokedAt = null,
            ExpiresAt = _now.UtcDateTime.AddDays(1)
        };

        _refreshTokens.FirstOrDefaultAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                      .Returns(storedToken);

        var user = new User { Id = 5, Email = "user@example.com", RoleId = 2, IsActive = true };
        _users.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(user);

        var issuedTokens = new IssuedTokens("new_access_token", _now.AddMinutes(15), "new_refresh_token", _now.AddDays(7), "new-jti");
        _tokenIssuer.Issue(Arg.Any<AccessTokenDescriptor>(), "TestBrowser", "127.0.0.1")
                    .Returns(issuedTokens);

        var cmd = new RefreshTokenCommand(rawToken);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_refresh_token");

        storedToken.RevokedAt.Should().Be(_now.UtcDateTime);
        storedToken.RevokedByIp.Should().Be("127.0.0.1");
        storedToken.ReplacedByToken.Should().Be(HashToken("new_refresh_token"));

        await _refreshTokens.Received(1).AddAsync(Arg.Is<RefreshToken>(r =>
            r.UserId == 5 &&
            r.Token == HashToken("new_refresh_token")
        ), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
