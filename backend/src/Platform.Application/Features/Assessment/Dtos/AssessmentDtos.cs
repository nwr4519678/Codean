using System;
using System.Collections.Generic;
using MediatR;
using Platform.Domain.Results;

namespace Platform.Application.Features.Assessment.Dtos;

// ── Exam DTOs ─────────────────────────────────────────────────────────────

public sealed record ExamResponse(
    long Id,
    long TeacherId,
    long? CourseId,
    string Title,
    string Description,
    int DurationMinutes,
    decimal TotalMarks,
    decimal? PassingMarks,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsPublished,
    DateTime CreatedAt
);

public sealed record CreateExamCommand(
    long? CourseId,
    string Title,
    string Description,
    int DurationMinutes,
    decimal TotalMarks,
    decimal? PassingMarks,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<Result<ExamResponse>>;

public sealed record UpdateExamCommand(
    long ExamId,
    string Title,
    string Description,
    int DurationMinutes,
    decimal TotalMarks,
    decimal? PassingMarks,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<Result<ExamResponse>>;

public sealed record PublishExamCommand(long ExamId, bool IsPublished) : IRequest<Result<bool>>;
public sealed record GetExamByIdQuery(long ExamId) : IRequest<Result<ExamResponse>>;

public sealed record GetExamsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    long? CourseId = null,
    long? TeacherId = null,
    bool? IsPublished = null
) : IRequest<Result<Platform.Application.Common.Pagination.PagedList<ExamResponse>>>;


// ── Exam Attempt DTOs ─────────────────────────────────────────────────────

public sealed record ExamAttemptResponse(
    long Id,
    long ExamId,
    long StudentId,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    string Status,
    decimal? Score
);

public sealed record StartExamAttemptCommand(long ExamId) : IRequest<Result<ExamAttemptResponse>>;

public sealed record SubmitExamAnswerItem(long QuestionId, string AnswerText, string SelectedChoiceIds);

public sealed record SubmitExamAttemptCommand(
    long AttemptId,
    IReadOnlyList<SubmitExamAnswerItem> Answers
) : IRequest<Result<ExamAttemptResponse>>;

public sealed record GetExamAttemptByIdQuery(long AttemptId) : IRequest<Result<ExamAttemptResponse>>;


// ── Homework DTOs ─────────────────────────────────────────────────────────

public sealed record HomeworkResponse(
    long Id,
    long TeacherId,
    long? CourseId,
    long? LessonId,
    string Title,
    string Description,
    DateTime? DueDate,
    decimal TotalMarks,
    DateTime CreatedAt
);

public sealed record CreateHomeworkCommand(
    long? CourseId,
    long? LessonId,
    string Title,
    string Description,
    DateTime? DueDate,
    decimal TotalMarks
) : IRequest<Result<HomeworkResponse>>;

public sealed record UpdateHomeworkCommand(
    long HomeworkId,
    string Title,
    string Description,
    DateTime? DueDate,
    decimal TotalMarks
) : IRequest<Result<HomeworkResponse>>;

public sealed record DeleteHomeworkCommand(long HomeworkId) : IRequest<Result<bool>>;


// ── Homework Submission DTOs ──────────────────────────────────────────────

public sealed record HomeworkSubmissionResponse(
    long Id,
    long HomeworkId,
    long StudentId,
    string SubmissionType,
    string FileUrl,
    string TextAnswer,
    DateTime SubmittedAt,
    decimal? Grade,
    string Feedback,
    string Status
);

public sealed record SubmitHomeworkCommand(
    long HomeworkId,
    string SubmissionType,
    string FileUrl,
    string TextAnswer
) : IRequest<Result<HomeworkSubmissionResponse>>;

public sealed record GradeHomeworkSubmissionCommand(
    long SubmissionId,
    decimal Grade,
    string Feedback
) : IRequest<Result<HomeworkSubmissionResponse>>;

public sealed record GetHomeworkSubmissionsQuery(long HomeworkId) : IRequest<Result<IReadOnlyList<HomeworkSubmissionResponse>>>;
