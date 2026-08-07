using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Assessment.Dtos;
using Platform.Application.Features.Assessment.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Assessment.Commands;

public sealed class CreateExamHandler : IRequestHandler<CreateExamCommand, Result<ExamResponse>>
{
    private readonly IRepository<Exam> _exams;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateExamHandler(
        IRepository<Exam> exams,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _exams = exams;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ExamResponse>> Handle(CreateExamCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<ExamResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var exam = new Exam
        {
            TeacherId = _currentUser.UserId.Value,
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DurationMinutes = request.DurationMinutes,
            TotalMarks = request.TotalMarks,
            PassingMarks = request.PassingMarks,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsPublished = false,
            CreatedAt = _clock.UtcNow.UtcDateTime
        };

        await _exams.AddAsync(exam, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ExamResponse>.Success(exam.ToResponse());
    }
}

public sealed class UpdateExamHandler : IRequestHandler<UpdateExamCommand, Result<ExamResponse>>
{
    private readonly IRepository<Exam> _exams;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateExamHandler(
        IRepository<Exam> exams,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _exams = exams;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<ExamResponse>> Handle(UpdateExamCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<ExamResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null)
            return Result<ExamResponse>.Failure(Error.NotFound("exams.not_found", $"Exam {request.ExamId} not found."));

        if (exam.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<ExamResponse>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission."));

        exam.Title = request.Title.Trim();
        exam.Description = request.Description?.Trim();
        exam.DurationMinutes = request.DurationMinutes;
        exam.TotalMarks = request.TotalMarks;
        exam.PassingMarks = request.PassingMarks;
        exam.StartDate = request.StartDate;
        exam.EndDate = request.EndDate;

        _exams.Update(exam);
        await _uow.SaveChangesAsync(ct);

        return Result<ExamResponse>.Success(exam.ToResponse());
    }
}

public sealed class PublishExamHandler : IRequestHandler<PublishExamCommand, Result<bool>>
{
    private readonly IRepository<Exam> _exams;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public PublishExamHandler(
        IRepository<Exam> exams,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _exams = exams;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(PublishExamCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null)
            return Result<bool>.Failure(Error.NotFound("exams.not_found", $"Exam {request.ExamId} not found."));

        if (exam.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission."));

        exam.IsPublished = request.IsPublished;
        _exams.Update(exam);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public sealed class GetExamByIdHandler : IRequestHandler<GetExamByIdQuery, Result<ExamResponse>>
{
    private readonly IRepository<Exam> _exams;
    public GetExamByIdHandler(IRepository<Exam> exams) => _exams = exams;

    public async Task<Result<ExamResponse>> Handle(GetExamByIdQuery request, CancellationToken ct)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null)
            return Result<ExamResponse>.Failure(Error.NotFound("exams.not_found", $"Exam {request.ExamId} not found."));

        return Result<ExamResponse>.Success(exam.ToResponse());
    }
}

public sealed class GetExamsPagedHandler : IRequestHandler<GetExamsPagedQuery, Result<PagedList<ExamResponse>>>
{
    private readonly IRepository<Exam> _exams;
    public GetExamsPagedHandler(IRepository<Exam> exams) => _exams = exams;

    public async Task<Result<PagedList<ExamResponse>>> Handle(GetExamsPagedQuery request, CancellationToken ct)
    {
        var q = _exams.Query();

        if (request.CourseId.HasValue)
            q = q.Where(e => e.CourseId == request.CourseId.Value);

        if (request.TeacherId.HasValue)
            q = q.Where(e => e.TeacherId == request.TeacherId.Value);

        if (request.IsPublished.HasValue)
            q = q.Where(e => e.IsPublished == request.IsPublished.Value);

        q = q.OrderByDescending(e => e.CreatedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(e => e.ToResponse())
                     .ToList();

        return Result<PagedList<ExamResponse>>.Success(new PagedList<ExamResponse>(items, request.PageNumber, request.PageSize, total));
    }
}
