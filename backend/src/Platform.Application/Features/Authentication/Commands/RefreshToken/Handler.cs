using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;
using Platform.Domain.Security;

namespace Platform.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokens;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public RefreshTokenHandler(
        IRepository<User> users,
        IRepository<Domain.Entities.RefreshToken> refreshTokens,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ITokenIssuer tokenIssuer,
        ICurrentUser current,
        IClock clock)
    {
        _users         = users;
        _refreshTokens = refreshTokens;
        _auditLogs     = auditLogs;
        _uow           = uow;
        _tokenIssuer   = tokenIssuer;
        _current       = current;
        _clock         = clock;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var now        = _clock.UtcNow.UtcDateTime;
        var tokenHash  = HashToken(cmd.Token);

        var stored = await _refreshTokens.FirstOrDefaultAsync(t => t.Token == tokenHash, ct);

        if (stored is null)
            return Error.Unauthorized("auth.invalid_token", "Refresh token is invalid.");

        // ── Reuse Detection ──────────────────────────────────────────────────
        // A token that was already revoked means it's being reused — possible theft.
        if (stored.RevokedAt is not null)
        {
            var allActive = await _refreshTokens.ListAsync(
                t => t.UserId == stored.UserId && t.RevokedAt == null, ct);

            foreach (var t in allActive)
            {
                t.RevokedAt        = now;
                t.RevokedByIp      = _current.IpAddress;
                t.ReplacedByToken  = "SECURITY_REVOKED";
                _refreshTokens.Update(t);
            }

            await _auditLogs.AddAsync(new AuditLog
            {
                UserId     = stored.UserId,
                Action     = "Token.ReuseDetected",
                EntityType = "RefreshToken",
                EntityId   = stored.UserId,
                IpAddress  = _current.IpAddress,
                CreatedAt  = now,
                NewValues  = "{\"Alert\":\"Refresh token reuse detected — all tokens revoked\"}"
            }, ct);

            await _uow.SaveChangesAsync(ct);
            return Error.Unauthorized("auth.token_reuse", "Security alert: please log in again.");
        }

        if (stored.ExpiresAt < now)
            return Error.Unauthorized("auth.token_expired", "Refresh token has expired.");

        var user = await _users.GetByIdAsync(stored.UserId, ct);
        if (user is null || !user.IsActive)
            return Error.Unauthorized("auth.account_disabled", "Account is disabled.");

        // ── Issue New Token Pair ─────────────────────────────────────────────
        var roleName    = ResolveRole(user.RoleId);
        var permissions = BuildPermissions(roleName);
        var descriptor  = new AccessTokenDescriptor(user.Id, user.Email, [roleName], permissions, TimeSpan.FromMinutes(15));
        var issued      = _tokenIssuer.Issue(descriptor, _current.UserAgent, _current.IpAddress);

        // Rotate: revoke old token, store new hash
        var newHash         = HashToken(issued.RefreshToken);
        stored.RevokedAt    = now;
        stored.RevokedByIp  = _current.IpAddress;
        stored.ReplacedByToken = newHash;
        _refreshTokens.Update(stored);

        await _refreshTokens.AddAsync(new Domain.Entities.RefreshToken
        {
            UserId      = stored.UserId,
            Token       = newHash,
            ExpiresAt   = issued.RefreshTokenExpiresAt.UtcDateTime,
            CreatedAt   = now,
            CreatedByIp = _current.IpAddress
        }, ct);

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = stored.UserId,
            Action     = "Token.Refreshed",
            EntityType = "RefreshToken",
            EntityId   = stored.UserId,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return issued.ToRefreshTokenResponse();
    }

    private static string ResolveRole(int roleId) => roleId switch { 2 => "Teacher", 3 => "Admin", _ => "Student" };

    private static List<string> BuildPermissions(string role)
    {
        var p = new List<string> { Permissions.CoursesRead, Permissions.LessonsRead };
        if (role is "Teacher" or "Admin") p.AddRange([Permissions.CoursesWrite, Permissions.ExamsGrade]);
        if (role is "Admin") p.AddRange([Permissions.UsersManage, Permissions.SystemSettingsManage]);
        return p;
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
