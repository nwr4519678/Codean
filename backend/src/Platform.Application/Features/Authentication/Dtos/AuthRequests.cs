using System;
using MediatR;
using Platform.Application.Common.Caching;
using Platform.Application.Features.Authentication.Caching;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Dtos;

// ═══════════════════════════════════════════════════════════════════════════════
//  Authentication — Request DTOs (Commands & Queries)
//  These are the MediatR request objects consumed by the Application handlers.
//  The API layer maps HTTP request bodies to these types via MediatR ISender.
// ═══════════════════════════════════════════════════════════════════════════════

// ── Registration ──────────────────────────────────────────────────────────────

public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string? Phone = null)
    : IRequest<Result<RegisterResponse>>;

// ── Login ─────────────────────────────────────────────────────────────────────

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false)
    : IRequest<Result<LoginResponse>>;

// ── Token Lifecycle ───────────────────────────────────────────────────────────

public sealed record RefreshTokenCommand(string Token)
    : IRequest<Result<RefreshTokenResponse>>;

public sealed record RevokeTokenCommand(string Token)
    : IRequest<Result>;

// ── Password Management ───────────────────────────────────────────────────────

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword)
    : IRequest<Result>;

public sealed record ForgotPasswordCommand(string Email)
    : IRequest<Result>;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword)
    : IRequest<Result>;

// ── Email Verification ────────────────────────────────────────────────────────

public sealed record VerifyEmailCommand(
    string Email,
    string Token)
    : IRequest<Result>;

// ── Two-Factor Authentication ─────────────────────────────────────────────────

public sealed record SetupTwoFactorCommand : IRequest<Result<SetupTwoFactorResponse>>;

public sealed record VerifyTwoFactorCommand(
    string Secret,
    string Code)
    : IRequest<Result>;

// ── Session Management ────────────────────────────────────────────────────────

public sealed record RevokeSessionCommand(long SessionId) : IRequest<Result>;

// ── Queries ───────────────────────────────────────────────────────────────────

/// <summary>
/// Returns the current user's profile.
/// Cached per user for 5 minutes — invalidated on VerifyEmail and ChangePassword.
/// </summary>
public sealed record GetCurrentUserQuery(long UserId)
    : IRequest<Result<CurrentUserResponse>>, ICacheableRequest
{
    public string CacheKey   => AuthCacheKeys.UserProfile(UserId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

/// <summary>
/// Returns all sessions for the current user.
/// Cached per user for 2 minutes — invalidated on logout, token revoke, or session revoke.
/// </summary>
public sealed record GetActiveSessionsQuery(long UserId)
    : IRequest<Result<System.Collections.Generic.List<SessionResponse>>>, ICacheableRequest
{
    public string CacheKey   => AuthCacheKeys.UserSessions(UserId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
}
