namespace Platform.Judge.Models;

public record TestCase(
    string Input,
    string ExpectedOutput,
    bool IsHidden = false
);

public record TestCaseResult(
    int TestCaseIndex,
    bool Passed,
    string? ActualOutput,
    string? ErrorOutput,
    double ExecutionTimeMs,
    long MemoryUsedKb,
    string? FailureReason,
    bool IsHidden
);
