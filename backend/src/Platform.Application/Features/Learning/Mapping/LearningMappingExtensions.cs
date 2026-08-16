using System.Linq;
using Platform.Application.Features.Learning.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Learning.Mapping;

public static class LearningMappingExtensions
{
    public static CourseResponse ToResponse(this Course course, string teacherName = "")
    {
        int moduleCount = course.CourseModules?.Count ?? 0;
        int lessonCount = course.CourseModules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0;

        return new CourseResponse(
            course.Id,
            course.TeacherId,
            string.IsNullOrWhiteSpace(teacherName) ? course.Teacher?.User?.FullName ?? string.Empty : teacherName,
            course.Title,
            course.Description ?? string.Empty,
            course.Thumbnail ?? string.Empty,
            course.Category ?? string.Empty,
            course.Price,
            course.IsPublished,
            course.CreatedAt,
            course.UpdatedAt,
            moduleCount,
            lessonCount
        );
    }

    public static CourseDetailResponse ToDetailResponse(this Course course, string teacherName = "")
    {
        var modules = course.CourseModules?
            .OrderBy(m => m.Order)
            .Select(m => m.ToResponse())
            .ToList() ?? [];

        return new CourseDetailResponse(
            course.Id,
            course.TeacherId,
            string.IsNullOrWhiteSpace(teacherName) ? course.Teacher?.User?.FullName ?? string.Empty : teacherName,
            course.Title,
            course.Description ?? string.Empty,
            course.Thumbnail ?? string.Empty,
            course.Category ?? string.Empty,
            course.Price,
            course.IsPublished,
            course.CreatedAt,
            course.UpdatedAt,
            modules
        );
    }

    public static CourseModuleResponse ToResponse(this CourseModule module)
    {
        var lessons = module.Lessons?
            .OrderBy(l => l.Order)
            .Select(l => l.ToResponse())
            .ToList() ?? [];

        return new CourseModuleResponse(
            module.Id,
            module.CourseId,
            module.Title,
            module.MonthNumber,
            module.Order,
            module.Description ?? string.Empty,
            module.CreatedAt,
            lessons
        );
    }

    public static LessonResponse ToResponse(this Lesson lesson)
    {
        var resources = lesson.LessonResources?
            .Select(r => r.ToResponse())
            .ToList() ?? [];

        return new LessonResponse(
            lesson.Id,
            lesson.ModuleId,
            lesson.Title,
            lesson.Description ?? string.Empty,
            lesson.Duration,
            lesson.Order,
            lesson.IsPublished,
            lesson.CreatedAt,
            lesson.UpdatedAt,
            resources
        );
    }

    public static LessonResourceResponse ToResponse(this LessonResource resource)
    {
        return new LessonResourceResponse(
            resource.Id,
            resource.LessonId,
            resource.ResourceType ?? string.Empty,
            resource.FileUrl ?? string.Empty,
            resource.FileName ?? string.Empty,
            resource.FileSizeBytes,
            resource.CreatedAt
        );
    }

    public static StudentProgressResponse ToResponse(this StudentProgress progress)
    {
        return new StudentProgressResponse(
            progress.Id,
            progress.StudentId,
            progress.LessonId,
            progress.Completion,
            progress.LastViewed,
            progress.WatchTime
        );
    }

    public static CourseEnrollmentResponse ToResponse(this CourseEnrollment enrollment, CourseProgressResponse? progress = null)
    {
        var course = enrollment.Course;
        return new CourseEnrollmentResponse(
            enrollment.Id,
            enrollment.CourseId,
            course?.Title ?? string.Empty,
            course?.Thumbnail ?? string.Empty,
            course?.Category ?? string.Empty,
            course?.Teacher?.User?.FullName ?? string.Empty,
            course?.Price ?? 0,
            enrollment.Status,
            enrollment.AccessType,
            enrollment.EnrolledAt,
            enrollment.CompletedAt,
            progress
        );
    }
}
