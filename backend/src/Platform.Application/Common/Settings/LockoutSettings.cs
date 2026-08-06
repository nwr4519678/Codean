namespace Platform.Application.Common.Settings;

/// <summary>
/// Business rule configuration for account lockout.
/// Lives in Application because lockout behavior IS business logic —
/// only the values come from infrastructure (appsettings.json).
/// Bound by Platform.Api: services.Configure&lt;LockoutSettings&gt;(config.GetSection(SectionName))
/// </summary>
public sealed class LockoutSettings
{
    public const string SectionName = "Lockout";

    /// <summary>Number of consecutive failed login attempts before the account is locked.</summary>
    public int MaxFailedAttempts { get; init; } = 5;

    /// <summary>How long the account stays locked after exceeding MaxFailedAttempts.</summary>
    public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(15);
}
