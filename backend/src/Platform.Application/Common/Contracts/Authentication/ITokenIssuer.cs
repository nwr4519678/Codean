using System;
using System.Collections.Generic;

namespace Platform.Application.Common.Contracts.Authentication;

public sealed record AccessTokenDescriptor(
    long UserId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    TimeSpan Lifetime);

public sealed record IssuedTokens(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string Jti);

/// <summary>
/// Token issuer contract for generating JWT access tokens and secure refresh token descriptors.
/// Single Responsibility: Token Issuance.
/// </summary>
public interface ITokenIssuer
{
    IssuedTokens Issue(AccessTokenDescriptor descriptor, string? userAgent = null, string? ip = null);
}

/// <summary>
/// Application port for token generation.
/// </summary>
public interface ITokenService : ITokenIssuer
{
}
