using System;
using System.Collections.Generic;
using Platform.Judge.Abstractions;
using Platform.Judge.Models;

namespace Platform.Judge.Services;

public class TestCaseEvaluator : ITestCaseEvaluator
{
    public (int PassedCount, List<TestCaseResult> Results) Evaluate(
        List<TestCase> testCases,
        string actualOutput,
        string? errorOutput,
        double executionTimeMs,
        long memoryUsedKb)
    {
        var results = new List<TestCaseResult>();
        if (testCases == null || testCases.Count == 0)
        {
            return (0, results);
        }

        int passed = 0;
        var normalizedActual = (actualOutput ?? string.Empty).Trim().Replace("\r\n", "\n");

        for (int i = 0; i < testCases.Count; i++)
        {
            var tc = testCases[i];
            var normalizedExpected = (tc.ExpectedOutput ?? string.Empty).Trim().Replace("\r\n", "\n");

            bool isPassed = string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal);
            if (isPassed) passed++;

            string? failureReason = isPassed ? null : "Actual output did not match expected test case output.";

            // For hidden test cases, do not expose internal expected/actual outputs
            string? safeActual = tc.IsHidden ? "[HIDDEN]" : actualOutput;
            string? safeError = tc.IsHidden ? null : errorOutput;

            results.Add(new TestCaseResult(
                TestCaseIndex: i + 1,
                Passed: isPassed,
                ActualOutput: safeActual,
                ErrorOutput: safeError,
                ExecutionTimeMs: executionTimeMs,
                MemoryUsedKb: memoryUsedKb,
                FailureReason: failureReason,
                IsHidden: tc.IsHidden
            ));
        }

        return (passed, results);
    }
}
