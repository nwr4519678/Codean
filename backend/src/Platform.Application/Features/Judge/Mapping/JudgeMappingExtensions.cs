using Platform.Application.Features.Judge.Dtos;
using Platform.Domain.Entities;
using Platform.Application.Common.Contracts.Judge;

namespace Platform.Application.Features.Judge.Mapping;

public static class JudgeMappingExtensions
{
    public static CodingChallengeResponse ToResponse(this CodingChallenge challenge) =>
        new(
            challenge.Id,
            challenge.TeacherId,
            challenge.Title,
            challenge.Description ?? string.Empty,
            challenge.Language ?? string.Empty,
            challenge.StarterCode ?? string.Empty,
            challenge.Difficulty ?? string.Empty,
            challenge.Marks,
            challenge.CreatedAt
        );

    public static CodeSubmissionResultResponse ToResultResponse(
        this CodingSubmission submission,
        CodeExecutionResult? executionResult = null) =>
        new(
            submission.Id,
            executionResult?.ExecutionId ?? string.Empty,
            executionResult?.Status.ToString() ?? submission.Status ?? "Pending",
            executionResult?.Verdict?.ToString(),
            executionResult?.PassedTestCases ?? 0,
            executionResult?.TotalTestCases ?? 0,
            executionResult?.ExecutionTimeMs ?? 0,
            executionResult?.MemoryUsedKb ?? 0,
            executionResult?.CompilationOutput,
            executionResult?.FailureReason
        );
}
