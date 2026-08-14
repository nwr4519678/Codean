using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Settings;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Events.Authentication;
using Platform.Domain.Results;
using Platform.Domain.Security;

namespace Platform.Application.Features.Authentication.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokens;
    private readonly IRepository<UserSession> _userSessions;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;
    private readonly LockoutSettings _lockout;

    public LoginHandler(
        IRepository<User> users,
        IRepository<Role> roles,
        IRepository<Domain.Entities.RefreshToken> refreshTokens,
        IRepository<UserSession> userSessions,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ITokenIssuer tokenIssuer,
        ICurrentUser current,
        IClock clock,
        IOptions<LockoutSettings> lockout)
    {
        _users         = users;
        _roles         = roles;
        _refreshTokens = refreshTokens;
        _userSessions  = userSessions;
        _auditLogs     = auditLogs;
        _uow           = uow;
        _hasher        = hasher;
        _tokenIssuer   = tokenIssuer;
        _current       = current;
        _clock         = clock;
        _lockout       = lockout.Value;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var normalized = cmd.Email.Trim().ToLowerInvariant();
        var user       = await _users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
        var now        = _clock.UtcNow.UtcDateTime;

        // ── Lockout check (before password verify — avoids unnecessary bcrypt cost) ──
        if (user is not null && user.LockoutEnd.HasValue && user.LockoutEnd > now)
        {
            var remaining = (user.LockoutEnd.Value - now).TotalMinutes;
            return Error.Forbidden("auth.locked_out",
                $"Account is temporarily locked. Try again in {remaining:F0} minute(s).");
        }

        // ── Constant-time rejection (always verify even if user is null) ─────────
        var passwordValid = user is not null && _hasher.Verify(cmd.Password, user.PasswordHash);

        if (!passwordValid)
        {
            if (user is not null)
            {
                user.FailedLoginCount++;

                if (user.FailedLoginCount >= _lockout.MaxFailedAttempts)
                {
                    user.LockoutEnd   = now + _lockout.LockoutDuration;
                    user.FailedLoginCount = 0; // reset counter after lockout triggers
                }

                user.UpdatedAt = now;
                _users.Update(user);

                await RecordAuditAsync(user.Id, "User.LoginFailed",
                    $"IP:{_current.IpAddress} | Attempt:{user.FailedLoginCount}", now, ct);

                await _uow.SaveChangesAsync(ct);
            }

            return Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");
        }

        if (!user!.IsActive)
            return Error.Forbidden("auth.account_disabled", "Your account has been disabled.");

        // ── Issue tokens ──────────────────────────────────────────────────────────
        var role        = await _roles.FirstOrDefaultAsync(r => r.Id == user.RoleId, ct);
        var roleName    = role?.Name ?? "Student";
        var permissions = BuildPermissions(roleName);
        var lifetime    = cmd.RememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromMinutes(15);

        var descriptor = new AccessTokenDescriptor(user.Id, user.Email, [roleName], permissions, lifetime);
        var issued     = _tokenIssuer.Issue(descriptor, _current.UserAgent, _current.IpAddress);

        // Store SHA-256 hash of refresh token — never the raw value
        var refreshHash = HashToken(issued.RefreshToken);
        await _refreshTokens.AddAsync(new Domain.Entities.RefreshToken
        {
            User        = user,
            Token       = refreshHash,
            ExpiresAt   = issued.RefreshTokenExpiresAt.UtcDateTime,
            CreatedAt   = now,
            CreatedByIp = _current.IpAddress
        }, ct);

        await _userSessions.AddAsync(new UserSession
        {
            User       = user,
            DeviceInfo = _current.UserAgent ?? "Unknown Device",
            IpAddress  = _current.IpAddress ?? "0.0.0.0",
            LoginAt    = now,
            IsActive   = true
        }, ct);

        // ── Reset lockout state on successful login ───────────────────────────────
        user.FailedLoginCount = 0;
        user.LockoutEnd       = null;
        user.LastLogin        = now;
        user.UpdatedAt        = now;
        _users.Update(user);

        await RecordAuditAsync(user.Id, "User.LoggedIn", $"IP:{_current.IpAddress}", now, ct);
        await _uow.SaveChangesAsync(ct);

        return user.ToLoginResponse(roleName, issued);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ResolveRole(int roleId) => roleId switch
    {
        2 => "Teacher",
        3 => "Admin",
        _ => "Student"
    };

    private static List<string> BuildPermissions(string role)
    {
        var p = new List<string> { Permissions.CoursesRead, Permissions.LessonsRead, Permissions.ExamsRead };

        if (role is "Teacher" or "Admin")
        {
            p.AddRange([
                Permissions.CoursesWrite,
                Permissions.CoursesManage,
                Permissions.LessonsWrite,
                Permissions.ExamsGrade,
                Permissions.ExamsManage,
                Permissions.HomeworksManage,
                Permissions.ChallengesManage,
                Permissions.AnnouncementsManage
                ,Permissions.LiveSessionsManage
            ]);
        }

        if (role is "Admin")
        {
            p.AddRange([
                Permissions.UsersRead,
                Permissions.UsersManage,
                Permissions.PlansManage,
                Permissions.AnalyticsRead,
                Permissions.SystemSettingsManage
            ]);
        }

        return p;
    }

    private async Task RecordAuditAsync(long userId, string action, string details,
        DateTime now, CancellationToken ct)
    {
        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = userId,
            Action     = action,
            EntityType = "User",
            EntityId   = userId,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now,
            NewValues  = $"{{\"Details\":\"{details}\"}}"
        }, ct);
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
