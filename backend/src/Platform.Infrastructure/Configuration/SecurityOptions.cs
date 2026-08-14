using System;
using System.Collections.Generic;

namespace Platform.Infrastructure.Configuration;

public sealed class SigningKeyOptions
{
    public string Kid { get; set; } = "default-key-id";
    public string Secret { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "platform";
    public string Audience { get; set; } = "platform";
    public string Secret { get; set; } = "";
    public List<SigningKeyOptions> SigningKeys { get; set; } = new();
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}

public sealed class LockoutOptions
{
    public const string SectionName = "Lockout";

    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int DefaultLockoutTimeSpanMinutes { get; set; } = 15;
}

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int RequiredLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
}

public sealed class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    public int TokenLifetimeMinutes { get; set; } = 30;
}

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public int TokenLifetimeMinutes { get; set; } = 30;
}
