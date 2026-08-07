using System;
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

public sealed class StartExamAttemptHandler : IRequestHandler<StartExamAttemptCommand, Result<ExamAttemptResponse>>
{
    private readonly IRepository<Exam> _exams;
    private readonly IRepository<ExamAttempt> _attempts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public StartExamAttemptHandler(
        IRepository<Exam> exams,
        IRepository<ExamAttempt> attempts,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _exams = exams;
        _attempts = attempts;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ExamAttemptResponse>> Handle(StartExamAttemptCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<ExamAttemptResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null)
            return Result<ExamAttemptResponse>.Failure(Error.NotFound("exams.not_found", $"Exam {request.ExamId} not found."));

        if (!exam.IsPublished)
            return Result<ExamAttemptResponse>.Failure(Error.Validation("exams.not_published", "Exam is not published yet."));

        var attempt = new ExamAttempt
        {
            ExamId = request.ExamId,
            StudentId = studentId,
            StartedAt = _clock.UtcNow.UtcDateTime,
            Status = "InProgress"
        };

        await _attempts.AddAsync(attempt, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ExamAttemptResponse>.Success(attempt.ToResponse());
    }
}

public sealed class SubmitExamAttemptHandler : IRequestHandler<SubmitExamAttemptCommand, Result<ExamAttemptResponse>>
{
    private readonly IRepository<ExamAttempt> _attempts;
    private readonly IRepository<ExamAnswer> _answers;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitExamAttemptHandler(
        IRepository<ExamAttempt> attempts,
        IRepository<ExamAnswer> answers,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _attempts = attempts;
        _answers = answers;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ExamAttemptResponse>> Handle(SubmitExamAttemptCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<ExamAttemptResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var attempt = await _attempts.GetByIdAsync(request.AttemptId, ct);
        if (attempt is null)
            return Result<ExamAttemptResponse>.Failure(Error.NotFound("attempts.not_found", $"Attempt {request.AttemptId} not found."));

        if (attempt.StudentId != _currentUser.UserId.Value)
            return Result<ExamAttemptResponse>.Failure(Error.Forbidden("auth.forbidden", "You do not own this attempt."));

        if (attempt.Status == "Submitted")
            return Result<ExamAttemptResponse>.Failure(Error.Conflict("attempts.already_submitted", "Attempt has already been submitted."));

        var now = _clock.UtcNow.UtcDateTime;
        attempt.SubmittedAt = now;
        attempt.Status = "Submitted";

        foreach (var a in request.Answers)
        {
            var answer = new ExamAnswer
            {
                ExamAttemptId = attempt.Id,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                SelectedChoiceIds = a.SelectedChoiceIds
            };
            await _answers.AddAsync(answer, ct);
        }

        _attempts.Update(attempt);
        await _uow.SaveChangesAsync(ct);

        return Result<ExamAttemptResponse>.Success(attempt.ToResponse());
    }
}

public sealed class GetExamAttemptByIdHandler : IRequestHandler<GetExamAttemptByIdQuery, Result<ExamAttemptResponse>>
{
    private readonly IRepository<ExamAttempt> _attempts;
    public GetExamAttemptByIdHandler(IRepository<ExamAttempt> attempts) => _attempts = attempts;

    public async Task<Result<ExamAttemptResponse>> Handle(GetExamAttemptByIdQuery request, CancellationToken ct)
    {
        var attempt = await _attempts.GetByIdAsync(request.AttemptId, ct);
        if (attempt is null)
            return Result<ExamAttemptResponse>.Failure(Error.NotFound("attempts.not_found", $"Attempt {request.AttemptId} not found."));

        return Result<ExamAttemptResponse>.Success(attempt.ToResponse());
    }
}
