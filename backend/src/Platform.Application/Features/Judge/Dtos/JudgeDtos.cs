using System;
using System.Collections.Generic;
using MediatR;
using Platform.Application.Common.Contracts.Judge;
using Platform.Domain.Results;

namespace Platform.Application.Features.Judge.Dtos;

// ── Coding Challenge DTOs ─────────────────────────────────────────────────

public sealed record CodingChallengeResponse(
    long Id,
    long TeacherId,
    string Title,
    string Description,
    string Language,
    string StarterCode,
    string Difficulty,
    decimal Marks,
    DateTime CreatedAt
);

public sealed record CreateCodingChallengeCommand(
    string Title,
    string Description,
    string Language,
    string StarterCode,
    string TestCases,
    string Difficulty,
    decimal Marks
) : IRequest<Result<CodingChallengeResponse>>;

public sealed record UpdateCodingChallengeCommand(
    long ChallengeId,
    string Title,
    string Description,
    string StarterCode,
    string TestCases,
    string Difficulty,
    decimal Marks
) : IRequest<Result<CodingChallengeResponse>>;

public sealed record GetCodingChallengeByIdQuery(long ChallengeId)
    : IRequest<Result<CodingChallengeResponse>>;

public sealed record GetCodingChallengesQuery()
    : IRequest<Result<IReadOnlyList<CodingChallengeResponse>>>;

// ── Code Submission DTOs ──────────────────────────────────────────────────

/// <summary>
/// Returned immediately after the external compiler finishes.
/// </summary>
public sealed record SubmitCodeResponse(
    long SubmissionId,
    string ExecutionId,
    string Status,
    DateTime SubmittedAt
);

/// <summary>
/// Detailed result returned by the external compiler adapter.
/// </summary>
public sealed record CodeSubmissionResultResponse(
    long SubmissionId,
    string ExecutionId,
    string Status,
    string? Verdict,
    int PassedTestCases,
    int TotalTestCases,
    double ExecutionTimeMs,
    long MemoryUsedKb,
    string? CompilationOutput,
    string? FailureReason
);

/// <summary>
/// Sends source code to the external compiler API and stores the result.
/// </summary>
public sealed record SubmitCodeChallengeCommand(
    long ChallengeId,
    string SourceCode,
    string Language
) : IRequest<Result<SubmitCodeResponse>>;

public sealed record GetSubmissionStatusQuery(long SubmissionId)
    : IRequest<Result<CodeSubmissionResultResponse>>;

public sealed record GetMySubmissionsQuery(long ChallengeId)
    : IRequest<Result<IReadOnlyList<CodeSubmissionResultResponse>>>;
