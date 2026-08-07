using Platform.Application.Features.Assessment.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Assessment.Mapping;

public static class AssessmentMappingExtensions
{
    public static ExamResponse ToResponse(this Exam exam)
    {
        return new ExamResponse(
            exam.Id,
            exam.TeacherId,
            exam.CourseId,
            exam.Title,
            exam.Description ?? string.Empty,
            exam.DurationMinutes,
            exam.TotalMarks,
            exam.PassingMarks,
            exam.StartDate,
            exam.EndDate,
            exam.IsPublished,
            exam.CreatedAt
        );
    }

    public static ExamAttemptResponse ToResponse(this ExamAttempt attempt)
    {
        return new ExamAttemptResponse(
            attempt.Id,
            attempt.ExamId,
            attempt.StudentId,
            attempt.StartedAt,
            attempt.SubmittedAt,
            attempt.Status ?? "In Progress",
            attempt.Score
        );
    }

    public static HomeworkResponse ToResponse(this Homework homework)
    {
        return new HomeworkResponse(
            homework.Id,
            homework.TeacherId,
            homework.CourseId,
            homework.LessonId,
            homework.Title,
            homework.Description ?? string.Empty,
            homework.DueDate,
            homework.TotalMarks,
            homework.CreatedAt
        );
    }

    public static HomeworkSubmissionResponse ToResponse(this HomeworkSubmission sub)
    {
        return new HomeworkSubmissionResponse(
            sub.Id,
            sub.HomeworkId,
            sub.StudentId,
            sub.SubmissionType ?? "File",
            sub.FileUrl ?? string.Empty,
            sub.TextAnswer ?? string.Empty,
            sub.SubmittedAt,
            sub.Grade,
            sub.Feedback ?? string.Empty,
            sub.Status ?? "Submitted"
        );
    }
}
