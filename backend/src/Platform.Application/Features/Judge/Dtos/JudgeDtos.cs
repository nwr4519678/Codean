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

// ── Code Submission DTOs ──────────────────────────────────────────────────

/// <summary>
/// Returned immediately after enqueuing. Clients poll for the result.
/// </summary>
public sealed record SubmitCodeResponse(
    long SubmissionId,
    string ExecutionId,
    string Status,   // "Queued"
    DateTime SubmittedAt
);

/// <summary>
/// Detailed result once the judge worker has completed processing.
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
/// Fire-and-forget: enqueues the submission to the judge, stores pending status.
/// Returns immediately with ExecutionId for polling.
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
