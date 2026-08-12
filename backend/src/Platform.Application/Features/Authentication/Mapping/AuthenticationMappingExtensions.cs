using System;
using System.Collections.Generic;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Authentication.Mapping;

/// <summary>
/// Pure static extension methods to map Domain entities and Infrastructure results
/// → Authentication response DTOs (from Dtos/AuthResponses.cs).
///
/// Used directly in handlers — no IMapper injection required.
/// AutoMapper profile (AuthenticationProfile) remains for any query-level projections.
/// </summary>
public static class AuthenticationMappingExtensions
{
    // ── User → RegisterResponse ───────────────────────────────────────────────

    public static RegisterResponse ToRegisterResponse(this User user) =>
        new(
            UserId:                    user.Id,
            Email:                     user.Email,
            FullName:                  user.FullName,
            EmailVerificationRequired: !user.EmailConfirmed);

    // ── User + IssuedTokens → LoginResponse ───────────────────────────────────

    public static LoginResponse ToLoginResponse(
        this User user,
        string roleName,
        IssuedTokens issued) =>
        new(
            UserId:                  user.Id,
            Email:                   user.Email,
            FullName:                user.FullName,
            Role:                    roleName,
            AccessToken:             issued.AccessToken,
            RefreshToken:            issued.RefreshToken,
            AccessTokenExpiresAt:    issued.AccessTokenExpiresAt,
            RefreshTokenExpiresAt:   issued.RefreshTokenExpiresAt);

    // ── IssuedTokens → RefreshTokenResponse ──────────────────────────────────

    public static RefreshTokenResponse ToRefreshTokenResponse(this IssuedTokens issued) =>
        new(
            AccessToken:           issued.AccessToken,
            RefreshToken:          issued.RefreshToken,
            AccessTokenExpiresAt:  issued.AccessTokenExpiresAt,
            RefreshTokenExpiresAt: issued.RefreshTokenExpiresAt);

    // ── User → CurrentUserResponse ────────────────────────────────────────────

    public static CurrentUserResponse ToCurrentUserResponse(this User user, string roleName = "Student") =>
        new(
            UserId:         user.Id,
            Email:          user.Email,
            FullName:       user.FullName,
            Phone:          user.Phone,
            Role:           roleName,
            EmailConfirmed: user.EmailConfirmed,
            LastLogin:      user.LastLogin,
            CreatedAt:      user.CreatedAt);

    // ── (secret, qrUri, backupCodes) → SetupTwoFactorResponse ────────────────

    public static SetupTwoFactorResponse ToSetupTwoFactorResponse(
        string secret,
        string qrCodeUri,
        string[] backupCodes) =>
        new(
            Secret:      secret,
            QrCodeUri:   qrCodeUri,
            BackupCodes: backupCodes);

    // ── UserSession → SessionResponse ─────────────────────────────────────────

    public static SessionResponse ToSessionResponse(this UserSession session, string? currentIp) =>
        new(
            SessionId:  session.Id,
            DeviceInfo: session.DeviceInfo ?? "Unknown Device",
            IpAddress:  session.IpAddress  ?? "Unknown",
            LoginAt:    session.LoginAt,
            LogoutAt:   session.LogoutAt,
            IsActive:   session.IsActive,
            IsCurrent:  session.IsActive && session.IpAddress == currentIp);

    public static List<SessionResponse> ToSessionResponseList(
        this List<UserSession> sessions,
        string? currentIp)
        => sessions.ConvertAll(s => s.ToSessionResponse(currentIp));
}
