using System.Collections.Generic;
using Platform.Judge.Contracts;

namespace Platform.Judge.Models;

public record ExecutionRequest(
    string ExecutionId,
    long SubmissionId,
    string Language,
    string SourceCode,
    string? StandardInput = null,
    List<TestCase>? TestCases = null,
    ExecutionLimits? Limits = null
);
