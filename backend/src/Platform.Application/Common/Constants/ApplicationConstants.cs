namespace Platform.Application.Common.Constants;

public static class ApplicationConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Instructor = "Instructor";
        public const string Student = "Student";
    }

    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireInstructor = "RequireInstructor";
        public const string RequireStudent = "RequireStudent";
    }

    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }
}
