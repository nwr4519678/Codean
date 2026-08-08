namespace Platform.Domain.Security;

/// <summary>
/// Fine-grained permission string constants for policy-based authorization.
/// </summary>
public static class Permissions
{
    public const string UsersRead = "users:read";
    public const string UsersManage = "users.manage";

    public const string CoursesRead = "courses:read";
    public const string CoursesWrite = "courses:write";
    public const string CoursesManage = "courses.manage";

    public const string LessonsRead = "lessons:read";
    public const string LessonsWrite = "lessons:write";

    public const string ExamsRead = "exams:read";
    public const string ExamsGrade = "exams:grade";
    public const string ExamsManage = "exams.manage";

    public const string HomeworksManage = "homeworks.manage";
    public const string ChallengesManage = "challenges.manage";
    public const string AnnouncementsManage = "announcements.manage";
    public const string PlansManage = "plans.manage";

    public const string AnalyticsRead = "analytics:read";
    public const string SystemSettingsManage = "system:settings:manage";
}
