using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Learning.Dtos;
using Platform.Application.Features.Learning.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Learning.Commands;

public sealed class TrackLessonProgressHandler : IRequestHandler<TrackLessonProgressCommand, Result<StudentProgressResponse>>
{
    private readonly IRepository<Lesson> _lessons;
    private readonly IRepository<StudentProgress> _progresses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public TrackLessonProgressHandler(
        IRepository<Lesson> lessons,
        IRepository<StudentProgress> progresses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _lessons = lessons;
        _progresses = progresses;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<StudentProgressResponse>> Handle(TrackLessonProgressCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<StudentProgressResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var lesson = await _lessons.GetByIdAsync(request.LessonId, ct);
        if (lesson is null)
            return Result<StudentProgressResponse>.Failure(Error.NotFound("lessons.not_found", $"Lesson {request.LessonId} not found."));

        var progress = await _progresses.FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == request.LessonId, ct);
        var now = _clock.UtcNow.UtcDateTime;

        if (progress is null)
        {
            progress = new StudentProgress
            {
                StudentId = studentId,
                LessonId = request.LessonId,
                Completion = request.Completion,
                LastViewed = now,
                WatchTime = request.WatchTime
            };
            await _progresses.AddAsync(progress, ct);
        }
        else
        {
            progress.Completion = System.Math.Max(progress.Completion, request.Completion);
            progress.WatchTime += request.WatchTime;
            progress.LastViewed = now;
            _progresses.Update(progress);
        }

        await _uow.SaveChangesAsync(ct);

        return Result<StudentProgressResponse>.Success(progress.ToResponse());
    }
}

public sealed class GetCourseProgressHandler : IRequestHandler<GetCourseProgressQuery, Result<CourseProgressResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IRepository<StudentProgress> _progresses;
    private readonly ICurrentUser _currentUser;

    public GetCourseProgressHandler(
        IRepository<Course> courses,
        IRepository<StudentProgress> progresses,
        ICurrentUser currentUser)
    {
        _courses = courses;
        _progresses = progresses;
        _currentUser = currentUser;
    }

    public async Task<Result<CourseProgressResponse>> Handle(GetCourseProgressQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseProgressResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<CourseProgressResponse>.Failure(Error.NotFound("courses.not_found", $"Course {request.CourseId} not found."));

        var allLessonIds = course.CourseModules?
            .SelectMany(m => m.Lessons ?? [])
            .Select(l => l.Id)
            .ToList() ?? [];

        int totalLessons = allLessonIds.Count;

        var progressRecords = await _progresses.ListAsync(p => p.StudentId == studentId && allLessonIds.Contains(p.LessonId), ct);

        int completedLessons = progressRecords.Count(p => p.Completion >= 100);
        decimal overallPercentage = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;

        var dtos = progressRecords.Select(p => p.ToResponse()).ToList();

        var response = new CourseProgressResponse(
            request.CourseId,
            totalLessons,
            completedLessons,
            System.Math.Round(overallPercentage, 2),
            dtos
        );

        return Result<CourseProgressResponse>.Success(response);
    }
}
