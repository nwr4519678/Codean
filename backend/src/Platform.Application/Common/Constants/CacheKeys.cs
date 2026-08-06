namespace Platform.Application.Common.Constants;

public static class CacheKeys
{
    public static string UserProfile(long userId) => $"users:{userId}:profile";
    public static string UserRoles(long userId) => $"users:{userId}:roles";
    public static string CourseDetails(long courseId) => $"courses:{courseId}:details";
    public static string LiveSession(string sessionId) => $"sessions:{sessionId}";
}
