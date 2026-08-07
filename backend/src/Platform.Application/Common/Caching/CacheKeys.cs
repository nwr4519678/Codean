namespace Platform.Application.Common.Caching;

/// <summary>
/// Centralized cache key and tag constants.
/// Tag-based invalidation: write commands call ICacheService.RemoveByTagAsync(tag)
/// to bust all entries tagged under a group (e.g. all course list pages).
/// </summary>
public static class CacheKeys
{
    // ── Identity ──────────────────────────────────────────────────────────────
    public static string UserProfile(long userId) => $"auth:user:{userId}";
    public static string UserSessions(long userId) => $"auth:sessions:{userId}";

    // ── Learning ──────────────────────────────────────────────────────────────
    public static string CourseById(long courseId) => $"learning:course:{courseId}";
    public static string CoursesPaged(int pageNumber, int pageSize, long? teacherId, bool? isPublished) =>
        $"learning:courses:p_{pageNumber}_s_{pageSize}_t_{teacherId}_pub_{isPublished}";

    // ── Commerce ──────────────────────────────────────────────────────────────
    public static string SubscriptionPlans(long? teacherId, bool? isActive) =>
        $"commerce:plans:t_{teacherId}_active_{isActive}";
    public static string SubscriptionPlanById(long planId) =>
        $"commerce:plan:{planId}";

    // ── Communication ─────────────────────────────────────────────────────────
    public static string AnnouncementsPaged(int pageNumber, int pageSize, long? courseId, long? teacherId) =>
        $"comm:announcements:p_{pageNumber}_s_{pageSize}_c_{courseId}_t_{teacherId}";
    public static string UnreadNotificationCount(long userId) =>
        $"comm:notifications:unread_count:{userId}";

    // ── Analytics ─────────────────────────────────────────────────────────────
    public static string PlatformOverview => "analytics:platform_overview";
}

/// <summary>
/// Cache group tags used for bulk invalidation via <c>ICacheService.RemoveByTagAsync</c>.
/// Any entry stored with a tag can be invalidated as a group when the underlying data changes.
/// </summary>
public static class CacheTags
{
    /// <summary>All course list pages (GetCoursesPagedQuery entries).</summary>
    public const string CourseList = "tag:learning:courses_list";

    /// <summary>Single course detail responses (GetCourseByIdQuery).</summary>
    public static string CourseDetail(long courseId) => $"tag:learning:course:{courseId}";

    /// <summary>Subscription plan list pages.</summary>
    public const string SubscriptionPlanList = "tag:commerce:plans_list";

    /// <summary>Single subscription plan detail.</summary>
    public static string SubscriptionPlanDetail(long planId) => $"tag:commerce:plan:{planId}";

    /// <summary>All announcement list pages.</summary>
    public const string AnnouncementList = "tag:comm:announcements_list";

    /// <summary>Platform overview analytics.</summary>
    public const string PlatformOverview = "tag:analytics:overview";
}
