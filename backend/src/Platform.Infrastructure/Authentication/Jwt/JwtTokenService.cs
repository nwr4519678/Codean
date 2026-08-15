using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Infrastructure.Configuration;

namespace Platform.Infrastructure.Authentication.Jwt;

public sealed class JwtTokenService : ITokenIssuer
{
    private readonly JwtOptions _opt;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value;
    }

    public IssuedTokens Issue(AccessTokenDescriptor descriptor, string? userAgent = null, string? ip = null)
    {
        var now        = DateTimeOffset.UtcNow;
        // Honour the descriptor lifetime (allows per-request overrides like RememberMe)
        var lifetime   = descriptor.Lifetime > TimeSpan.Zero
            ? descriptor.Lifetime
            : TimeSpan.FromMinutes(_opt.AccessTokenLifetimeMinutes);
        var accessExp  = now.Add(lifetime);
        var refreshExp = now.AddDays(_opt.RefreshTokenLifetimeDays);
        var jti = Guid.NewGuid().ToString();

        // Active Signing Key Selection
        // Prefer an active key that is actually configured. Production hosts may
        // still provide the legacy Jwt:Secret while the template SigningKeys
        // entry remains present with an empty secret.
        var activeKeyOpt = _opt.SigningKeys.FirstOrDefault(k => k.IsActive && !string.IsNullOrWhiteSpace(k.Secret))
                           ?? new SigningKeyOptions { Secret = _opt.Secret, Kid = "default-kid", IsActive = true };

        if (string.IsNullOrWhiteSpace(activeKeyOpt.Secret))
            throw new InvalidOperationException("JWT signing key is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(activeKeyOpt.Secret));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, descriptor.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, descriptor.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(ClaimTypes.NameIdentifier, descriptor.UserId.ToString())
        };

        if (descriptor.Roles != null)
        {
            claims.AddRange(descriptor.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        if (descriptor.Permissions != null)
        {
            claims.AddRange(descriptor.Permissions.Select(p => new Claim("permission", p)));
        }

        var header = new JwtHeader(signingCredentials);
        header["kid"] = activeKeyOpt.Kid;

        var payload = new JwtPayload(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessExp.UtcDateTime);

        var token = new JwtSecurityToken(header, payload);
        var accessTokenString = _handler.WriteToken(token);

        // Opaque Refresh Token
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshTokenString = Convert.ToHexString(refreshTokenBytes).ToLowerInvariant();

        return new IssuedTokens(accessTokenString, accessExp, refreshTokenString, refreshExp, jti);
    }
}
