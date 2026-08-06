using System;
using System.Collections.Generic;

namespace Platform.Application.Features.Authentication.Dtos;

// ═══════════════════════════════════════════════════════════════════════════════
//  Authentication — Response DTOs
//  Immutable records returned from Application handlers to the API layer.
//  No infrastructure concerns, no EF/ASP.NET Core dependencies.
// ═══════════════════════════════════════════════════════════════════════════════

// ── Registration ──────────────────────────────────────────────────────────────

public sealed record RegisterResponse(
    long UserId,
    string Email,
    string FullName,
    bool EmailVerificationRequired);

// ── Login ─────────────────────────────────────────────────────────────────────

public sealed record LoginResponse(
    long UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);

// ── Token Refresh ─────────────────────────────────────────────────────────────

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);

// ── Current User ──────────────────────────────────────────────────────────────

public sealed record CurrentUserResponse(
    long UserId,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    bool EmailConfirmed,
    DateTime? LastLogin,
    DateTime CreatedAt);

// ── Two-Factor Authentication ─────────────────────────────────────────────────

public sealed record SetupTwoFactorResponse(
    string Secret,
    string QrCodeUri,
    string[] BackupCodes);

// ── Session Management ────────────────────────────────────────────────────────

public sealed record SessionResponse(
    long SessionId,
    string DeviceInfo,
    string IpAddress,
    DateTime LoginAt,
    DateTime? LogoutAt,
    bool IsActive,
    bool IsCurrent);
