namespace Platform.Application.Features.Authentication.Caching;

/// <summary>
/// Centralized cache key definitions for the Authentication feature.
/// All cache keys follow the pattern: auth:user:{userId}:{resource}
/// </summary>
public static class AuthCacheKeys
{
    /// <summary>Cached current user profile — invalidated on email verify or password change.</summary>
    public static string UserProfile(long userId) => $"auth:user:{userId}:profile";

    /// <summary>Cached active sessions list — invalidated on logout, token/session revocation.</summary>
    public static string UserSessions(long userId) => $"auth:user:{userId}:sessions";
}
