using System.Collections.Generic;
using Platform.Judge.Models;

namespace Platform.Judge.Abstractions;

public interface ITestCaseEvaluator
{
    (int PassedCount, List<TestCaseResult> Results) Evaluate(List<TestCase> testCases, string actualOutput, string? errorOutput, double executionTimeMs, long memoryUsedKb);
}
