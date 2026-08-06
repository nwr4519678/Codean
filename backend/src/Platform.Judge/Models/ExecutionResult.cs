using System.Collections.Generic;
using Platform.Judge.Contracts;

namespace Platform.Judge.Models;

public record ExecutionResult(
    string ExecutionId,
    long SubmissionId,
    ExecutionStatus Status,
    Verdict? Verdict,
    InfrastructureFailure InfrastructureFailure,
    int PassedTestCases,
    int TotalTestCases,
    List<TestCaseResult> TestCaseResults,
    string? CompilationOutput,
    string? StandardOutput,
    string? StandardError,
    int? ExitCode,
    double ExecutionTimeMs,
    long MemoryUsedKb,
    string? FailureReason
);
