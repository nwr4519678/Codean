using System;
using System.Collections.Generic;
using MediatR;
using Platform.Domain.Results;

namespace Platform.Application.Features.Learning.Dtos;

// ── Course DTOs ─────────────────────────────────────────────────────────────

public sealed record CourseResponse(
    long Id,
    long TeacherId,
    string TeacherName,
    string Title,
    string Description,
    string Thumbnail,
    string Category,
    decimal Price,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ModuleCount,
    int LessonCount
);

public sealed record CourseDetailResponse(
    long Id,
    long TeacherId,
    string TeacherName,
    string Title,
    string Description,
    string Thumbnail,
    string Category,
    decimal Price,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<CourseModuleResponse> Modules
);

public sealed record CreateCourseCommand(
    string Title,
    string Description,
    string? Thumbnail,
    string Category,
    decimal Price
) : IRequest<Result<CourseResponse>>;

public sealed record UpdateCourseCommand(
    long CourseId,
    string Title,
    string Description,
    string? Thumbnail,
    string Category,
    decimal Price
) : IRequest<Result<CourseResponse>>;

public sealed record PublishCourseCommand(long CourseId) : IRequest<Result<bool>>;
public sealed record ArchiveCourseCommand(long CourseId) : IRequest<Result<bool>>;

public sealed record GetCourseByIdQuery(long CourseId)
    : IRequest<Result<CourseDetailResponse>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.CourseById(CourseId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.CourseDetail(CourseId)];
}

public sealed record GetCoursesPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? Category = null,
    long? TeacherId = null,
    bool? IsPublished = null
) : IRequest<Result<Platform.Application.Common.Pagination.PagedList<CourseResponse>>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.CoursesPaged(PageNumber, PageSize, TeacherId, IsPublished);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.CourseList];
}


// ── Course Module DTOs ──────────────────────────────────────────────────────

public sealed record CourseModuleResponse(
    long Id,
    long CourseId,
    string Title,
    int MonthNumber,
    int Order,
    string Description,
    DateTime CreatedAt,
    IReadOnlyList<LessonResponse> Lessons
);

public sealed record CreateCourseModuleCommand(
    long CourseId,
    string Title,
    int MonthNumber,
    int Order,
    string Description
) : IRequest<Result<CourseModuleResponse>>;

public sealed record UpdateCourseModuleCommand(
    long ModuleId,
    string Title,
    int MonthNumber,
    int Order,
    string Description
) : IRequest<Result<CourseModuleResponse>>;

public sealed record DeleteCourseModuleCommand(long ModuleId) : IRequest<Result<bool>>;


// ── Lesson DTOs ─────────────────────────────────────────────────────────────

public sealed record LessonResponse(
    long Id,
    long ModuleId,
    string Title,
    string Description,
    int? Duration,
    int Order,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<LessonResourceResponse> Resources
);

public sealed record LessonResourceResponse(
    long Id,
    long LessonId,
    string ResourceType,
    string FileUrl,
    string FileName,
    long? FileSizeBytes,
    DateTime CreatedAt
);

public sealed record CreateLessonCommand(
    long ModuleId,
    string Title,
    string Description,
    int? Duration,
    int Order
) : IRequest<Result<LessonResponse>>;

public sealed record UpdateLessonCommand(
    long LessonId,
    string Title,
    string Description,
    int? Duration,
    int Order,
    bool IsPublished
) : IRequest<Result<LessonResponse>>;

public sealed record PublishLessonCommand(long LessonId, bool IsPublished) : IRequest<Result<bool>>;
public sealed record DeleteLessonCommand(long LessonId) : IRequest<Result<bool>>;


// ── Lesson Resource DTOs ───────────────────────────────────────────────────

public sealed record AttachLessonResourceCommand(
    long LessonId,
    string ResourceType,
    string FileUrl,
    string FileName,
    long? FileSizeBytes
) : IRequest<Result<LessonResourceResponse>>;

public sealed record RemoveLessonResourceCommand(long ResourceId) : IRequest<Result<bool>>;


// ── Student Progress DTOs ──────────────────────────────────────────────────

public sealed record StudentProgressResponse(
    long Id,
    long StudentId,
    long LessonId,
    decimal Completion,
    DateTime? LastViewed,
    int WatchTime
);

public sealed record CourseProgressResponse(
    long CourseId,
    int TotalLessons,
    int CompletedLessons,
    decimal OverallCompletionPercentage,
    IReadOnlyList<StudentProgressResponse> LessonProgress
);

public sealed record TrackLessonProgressCommand(
    long LessonId,
    decimal Completion,
    int WatchTime
) : IRequest<Result<StudentProgressResponse>>;

public sealed record GetCourseProgressQuery(long CourseId) : IRequest<Result<CourseProgressResponse>>;
