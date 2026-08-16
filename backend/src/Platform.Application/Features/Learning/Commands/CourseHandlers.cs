using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Caching;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Learning.Dtos;
using Platform.Application.Features.Learning.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Learning.Commands;

public sealed class GetMyCourseEnrollmentsHandler
    : IRequestHandler<GetMyCourseEnrollmentsQuery, Result<IReadOnlyList<CourseEnrollmentResponse>>>
{
    private readonly IRepository<CourseEnrollment> _enrollments;
    private readonly IRepository<StudentProgress> _progresses;
    private readonly ICurrentUser _currentUser;

    public GetMyCourseEnrollmentsHandler(
        IRepository<CourseEnrollment> enrollments,
        IRepository<StudentProgress> progresses,
        ICurrentUser currentUser)
    {
        _enrollments = enrollments;
        _progresses = progresses;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CourseEnrollmentResponse>>> Handle(
        GetMyCourseEnrollmentsQuery request,
        CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<IReadOnlyList<CourseEnrollmentResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var enrollments = await _enrollments.Query()
            .Where(e => e.StudentId == _currentUser.UserId.Value && e.Status == "Active")
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.CourseModules)
                    .ThenInclude(m => m.Lessons)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(ct);

        var responses = new List<CourseEnrollmentResponse>(enrollments.Count);
        foreach (var enrollment in enrollments)
        {
            var lessonIds = enrollment.Course.CourseModules
                .SelectMany(m => m.Lessons)
                .Select(l => l.Id)
                .ToList();
            var progressRecords = await _progresses.ListAsync(
                p => p.StudentId == enrollment.StudentId && lessonIds.Contains(p.LessonId), ct);
            var progress = BuildProgress(enrollment.CourseId, lessonIds.Count, progressRecords);
            responses.Add(enrollment.ToResponse(progress));
        }

        return Result<IReadOnlyList<CourseEnrollmentResponse>>.Success(responses);
    }

    private static CourseProgressResponse BuildProgress(
        long courseId,
        int totalLessons,
        IReadOnlyList<StudentProgress> progressRecords)
    {
        var completedLessons = progressRecords.Count(p => p.Completion >= 100);
        var percentage = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;
        return new CourseProgressResponse(
            courseId,
            totalLessons,
            completedLessons,
            Math.Round(percentage, 2),
            progressRecords.Select(p => p.ToResponse()).ToList());
    }
}

public sealed class EnrollInCourseHandler
    : IRequestHandler<EnrollInCourseCommand, Result<CourseEnrollmentResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IRepository<CourseEnrollment> _enrollments;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public EnrollInCourseHandler(
        IRepository<Course> courses,
        IRepository<CourseEnrollment> enrollments,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _courses = courses;
        _enrollments = enrollments;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<CourseEnrollmentResponse>> Handle(EnrollInCourseCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseEnrollmentResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var course = await _courses.Query()
            .Include(c => c.Teacher).ThenInclude(t => t.User)
            .Include(c => c.CourseModules).ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.IsPublished, ct);
        if (course is null)
            return Result<CourseEnrollmentResponse>.Failure(
                Error.NotFound("courses.not_found", "This course is not available for enrollment."));

        if (course.Price > 0)
            return Result<CourseEnrollmentResponse>.Failure(
                Error.Validation("courses.payment_required", "This paid course must be completed through checkout before access is granted."));

        var studentId = _currentUser.UserId.Value;
        var existing = await _enrollments.FirstOrDefaultAsync(
            e => e.CourseId == course.Id && e.StudentId == studentId, ct);
        if (existing is not null)
        {
            existing.Course = course;
            if (existing.Status == "Active")
                return Result<CourseEnrollmentResponse>.Success(existing.ToResponse());

            existing.Status = "Active";
            existing.AccessType = "Free";
            existing.EnrolledAt = _clock.UtcNow.UtcDateTime;
            existing.CompletedAt = null;
            _enrollments.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return Result<CourseEnrollmentResponse>.Success(existing.ToResponse());
        }

        var enrollment = new CourseEnrollment
        {
            CourseId = course.Id,
            StudentId = studentId,
            Status = "Active",
            AccessType = "Free",
            EnrolledAt = _clock.UtcNow.UtcDateTime
        };
        await _enrollments.AddAsync(enrollment, ct);
        await _uow.SaveChangesAsync(ct);
        enrollment.Course = course;
        return Result<CourseEnrollmentResponse>.Success(enrollment.ToResponse());
    }
}

// ── CreateCourse ──────────────────────────────────────────────────────────

public sealed class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IRepository<TeacherProfile> _teachers;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public CreateCourseHandler(
        IRepository<Course> courses,
        IRepository<TeacherProfile> teachers,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _courses = courses;
        _teachers = teachers;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<CourseResponse>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long teacherId = _currentUser.UserId.Value;
        var now = _clock.UtcNow.UtcDateTime;

        // Auto-create TeacherProfile if missing (handles accounts created before auto-create was added)
        var profile = await _teachers.FirstOrDefaultAsync(t => t.UserId == teacherId, ct);
        if (profile is null)
        {
            await _teachers.AddAsync(new TeacherProfile
            {
                UserId     = teacherId,
                Biography  = string.Empty,
                IsVerified = true
            }, ct);
        }

        var course = new Course
        {
            TeacherId   = teacherId,
            Title       = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Thumbnail   = request.Thumbnail?.Trim(),
            Category    = request.Category.Trim(),
            Price       = request.Price,
            IsPublished = false,
            CreatedAt   = now,
            UpdatedAt   = now
        };

        await _courses.AddAsync(course, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.CourseList, ct);

        return Result<CourseResponse>.Success(course.ToResponse());
    }
}

// ── UpdateCourse ──────────────────────────────────────────────────────────

public sealed class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public UpdateCourseHandler(
        IRepository<Course> courses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _courses = courses;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<CourseResponse>> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<CourseResponse>.Failure(Error.NotFound("courses.not_found", $"Course with ID {request.CourseId} was not found."));

        if (course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<CourseResponse>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission to modify this course."));

        course.Title = request.Title.Trim();
        course.Description = request.Description?.Trim();
        course.Thumbnail = request.Thumbnail?.Trim();
        course.Category = request.Category.Trim();
        course.Price = request.Price;
        course.UpdatedAt = _clock.UtcNow.UtcDateTime;

        _courses.Update(course);
        await _uow.SaveChangesAsync(ct);

        // Invalidate both the specific detail entry and all list pages
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.CourseDetail(request.CourseId), ct),
            _cache.RemoveByTagAsync(CacheTags.CourseList, ct));

        return Result<CourseResponse>.Success(course.ToResponse());
    }
}

// ── PublishCourse ─────────────────────────────────────────────────────────

public sealed class PublishCourseHandler : IRequestHandler<PublishCourseCommand, Result<bool>>
{
    private readonly IRepository<Course> _courses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public PublishCourseHandler(
        IRepository<Course> courses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _courses = courses;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(PublishCourseCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<bool>.Failure(Error.NotFound("courses.not_found", $"Course with ID {request.CourseId} was not found."));

        if (course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission to modify this course."));

        course.IsPublished = true;
        course.UpdatedAt = _clock.UtcNow.UtcDateTime;

        _courses.Update(course);
        await _uow.SaveChangesAsync(ct);

        // Bust both the detail and all list pages so published status is immediately visible
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.CourseDetail(request.CourseId), ct),
            _cache.RemoveByTagAsync(CacheTags.CourseList, ct));

        return Result<bool>.Success(true);
    }
}

// ── ArchiveCourse ─────────────────────────────────────────────────────────

public sealed class ArchiveCourseHandler : IRequestHandler<ArchiveCourseCommand, Result<bool>>
{
    private readonly IRepository<Course> _courses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public ArchiveCourseHandler(
        IRepository<Course> courses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _courses = courses;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(ArchiveCourseCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<bool>.Failure(Error.NotFound("courses.not_found", $"Course with ID {request.CourseId} was not found."));

        if (course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission to modify this course."));

        course.IsPublished = false;
        course.UpdatedAt = _clock.UtcNow.UtcDateTime;

        _courses.Update(course);
        await _uow.SaveChangesAsync(ct);

        // Bust both the detail and all list pages so archived status is immediately visible
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.CourseDetail(request.CourseId), ct),
            _cache.RemoveByTagAsync(CacheTags.CourseList, ct));

        return Result<bool>.Success(true);
    }
}

// ── Queries: GetCourseById & GetCoursesPaged ──────────────────────────────

public sealed class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDetailResponse>>
{
    private readonly IRepository<Course> _courses;

    public GetCourseByIdHandler(IRepository<Course> courses)
    {
        _courses = courses;
    }

    public async Task<Result<CourseDetailResponse>> Handle(GetCourseByIdQuery request, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<CourseDetailResponse>.Failure(Error.NotFound("courses.not_found", $"Course {request.CourseId} not found."));

        return Result<CourseDetailResponse>.Success(course.ToDetailResponse());
    }
}

public sealed class GetCoursesPagedHandler : IRequestHandler<GetCoursesPagedQuery, Result<PagedList<CourseResponse>>>
{
    private readonly IRepository<Course> _courses;

    public GetCoursesPagedHandler(IRepository<Course> courses)
    {
        _courses = courses;
    }

    public async Task<Result<PagedList<CourseResponse>>> Handle(GetCoursesPagedQuery request, CancellationToken ct)
    {
        var q = _courses.Query();

        if (request.IsPublished.HasValue)
            q = q.Where(c => c.IsPublished == request.IsPublished.Value);

        if (request.TeacherId.HasValue)
            q = q.Where(c => c.TeacherId == request.TeacherId.Value);

        if (!string.IsNullOrWhiteSpace(request.Category))
            q = q.Where(c => c.Category == request.Category.Trim());

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            q = q.Where(c => c.Title.Contains(s) || c.Description.Contains(s));
        }

        q = q.OrderByDescending(c => c.CreatedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(c => c.ToResponse())
                     .ToList();

        var paged = new PagedList<CourseResponse>(items, total, request.PageNumber, request.PageSize);
        return Result<PagedList<CourseResponse>>.Success(paged);
    }
}
