using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Assessment.Dtos;
using Platform.Application.Features.Assessment.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Assessment.Commands;

public sealed class CreateHomeworkHandler : IRequestHandler<CreateHomeworkCommand, Result<HomeworkResponse>>
{
    private readonly IRepository<Homework> _homeworks;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateHomeworkHandler(
        IRepository<Homework> homeworks,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _homeworks = homeworks;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<HomeworkResponse>> Handle(CreateHomeworkCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<HomeworkResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var homework = new Homework
        {
            TeacherId = _currentUser.UserId.Value,
            CourseId = request.CourseId,
            LessonId = request.LessonId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            TotalMarks = request.TotalMarks,
            CreatedAt = _clock.UtcNow.UtcDateTime
        };

        await _homeworks.AddAsync(homework, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<HomeworkResponse>.Success(homework.ToResponse());
    }
}

public sealed class SubmitHomeworkHandler : IRequestHandler<SubmitHomeworkCommand, Result<HomeworkSubmissionResponse>>
{
    private readonly IRepository<Homework> _homeworks;
    private readonly IRepository<HomeworkSubmission> _submissions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitHomeworkHandler(
        IRepository<Homework> homeworks,
        IRepository<HomeworkSubmission> submissions,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _homeworks = homeworks;
        _submissions = submissions;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<HomeworkSubmissionResponse>> Handle(SubmitHomeworkCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<HomeworkSubmissionResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var homework = await _homeworks.GetByIdAsync(request.HomeworkId, ct);
        if (homework is null)
            return Result<HomeworkSubmissionResponse>.Failure(Error.NotFound("homeworks.not_found", $"Homework {request.HomeworkId} not found."));

        var submission = new HomeworkSubmission
        {
            HomeworkId = request.HomeworkId,
            StudentId = studentId,
            SubmissionType = request.SubmissionType.Trim(),
            FileUrl = request.FileUrl?.Trim(),
            TextAnswer = request.TextAnswer?.Trim(),
            SubmittedAt = _clock.UtcNow.UtcDateTime,
            Status = "Submitted"
        };

        await _submissions.AddAsync(submission, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<HomeworkSubmissionResponse>.Success(submission.ToResponse());
    }
}

public sealed class GradeHomeworkSubmissionHandler : IRequestHandler<GradeHomeworkSubmissionCommand, Result<HomeworkSubmissionResponse>>
{
    private readonly IRepository<HomeworkSubmission> _submissions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GradeHomeworkSubmissionHandler(
        IRepository<HomeworkSubmission> submissions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<HomeworkSubmissionResponse>> Handle(GradeHomeworkSubmissionCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<HomeworkSubmissionResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var submission = await _submissions.GetByIdAsync(request.SubmissionId, ct);
        if (submission is null)
            return Result<HomeworkSubmissionResponse>.Failure(Error.NotFound("submissions.not_found", $"Submission {request.SubmissionId} not found."));

        submission.Grade = request.Grade;
        submission.Feedback = request.Feedback?.Trim();
        submission.Status = "Graded";

        _submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);

        return Result<HomeworkSubmissionResponse>.Success(submission.ToResponse());
    }
}

public sealed class GetHomeworkSubmissionsHandler : IRequestHandler<GetHomeworkSubmissionsQuery, Result<IReadOnlyList<HomeworkSubmissionResponse>>>
{
    private readonly IRepository<HomeworkSubmission> _submissions;
    public GetHomeworkSubmissionsHandler(IRepository<HomeworkSubmission> submissions) => _submissions = submissions;

    public async Task<Result<IReadOnlyList<HomeworkSubmissionResponse>>> Handle(GetHomeworkSubmissionsQuery request, CancellationToken ct)
    {
        var list = await _submissions.ListAsync(s => s.HomeworkId == request.HomeworkId, ct);
        var dtos = list.Select(s => s.ToResponse()).ToList();
        return Result<IReadOnlyList<HomeworkSubmissionResponse>>.Success(dtos);
    }
}
