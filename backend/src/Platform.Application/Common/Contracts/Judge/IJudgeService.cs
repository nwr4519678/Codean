using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Application.Common.Contracts.Judge;

public enum ExecutionStatus
{
    Pending,
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum Verdict
{
    Accepted,
    WrongAnswer,
    CompilationError,
    RuntimeError,
    TimeLimitExceeded,
    MemoryLimitExceeded,
    OutputLimitExceeded,
    SecurityViolation
}

public enum InfrastructureFailure
{
    None,
    ProviderUnavailable,
    QueueTimeout,
    ExecutionInfrastructureFailure,
    InternalError
}

public record TestCaseDto(
    string Input,
    string ExpectedOutput,
    bool IsHidden = false
);

public record ExecutionLimitsDto(
    double CpuTimeLimitSec = 2.0,
    long MemoryLimitMb = 256,
    long OutputSizeLimitKb = 1024
);

public record CodeExecutionRequest(
    string ExecutionId,
    long SubmissionId,
    string Language,
    string SourceCode,
    string? StandardInput = null,
    List<TestCaseDto>? TestCases = null,
    ExecutionLimitsDto? Limits = null
);

public record ExecutionSubmissionResponse(
    string ExecutionId,
    long SubmissionId,
    ExecutionStatus Status,
    DateTime SubmittedAt
);

public record ExecutionStatusResponse(
    string ExecutionId,
    long SubmissionId,
    ExecutionStatus Status,
    DateTime SubmittedAt,
    DateTime? CompletedAt = null
);

public record TestCaseResultDto(
    int TestCaseIndex,
    bool Passed,
    string? ActualOutput,
    string? ErrorOutput,
    double ExecutionTimeMs,
    long MemoryUsedKb,
    string? FailureReason,
    bool IsHidden
);

public record CodeExecutionResult(
    string ExecutionId,
    long SubmissionId,
    ExecutionStatus Status,
    Verdict? Verdict,
    InfrastructureFailure InfrastructureFailure,
    int PassedTestCases,
    int TotalTestCases,
    List<TestCaseResultDto> TestCaseResults,
    string? CompilationOutput,
    string? StandardOutput,
    string? StandardError,
    int? ExitCode,
    double ExecutionTimeMs,
    long MemoryUsedKb,
    string? FailureReason
);

/// <summary>
/// Outbound port for the code-execution Judge service.
/// Implemented by Platform.Infrastructure via HTTP (JudgeHttpClient).
/// </summary>
public interface IJudgeService
{
    /// <summary>Submits a code execution request. ExecutionId is the idempotency key.</summary>
    Task<ExecutionSubmissionResponse> SubmitAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Checks the operational status of a submitted execution.</summary>
    Task<ExecutionStatusResponse?> GetStatusAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the final result of a completed execution.</summary>
    Task<CodeExecutionResult?> GetResultAsync(string executionId, CancellationToken cancellationToken = default);
}
