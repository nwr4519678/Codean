using System.Collections.Generic;
using Platform.Judge.Models;

namespace Platform.Judge.Engines.Browser;

public class BrowserTestRunner
{
    public (int PassedCount, List<TestCaseResult> Results) EvaluateBrowserAssertions(
        string htmlContent,
        string cssContent,
        string jsContent,
        List<TestCase> assertions)
    {
        // Simple DOM / CSS static & contract assertion evaluator for browser tasks
        var results = new List<TestCaseResult>();
        int passed = 0;

        for (int i = 0; i < assertions.Count; i++)
        {
            var tc = assertions[i];
            bool isMatch = false;
            string? failureMsg = null;

            // Simple pattern matching for DOM element / CSS / JS assertions
            if (!string.IsNullOrWhiteSpace(tc.ExpectedOutput))
            {
                if (htmlContent.Contains(tc.ExpectedOutput) || cssContent.Contains(tc.ExpectedOutput) || jsContent.Contains(tc.ExpectedOutput))
                {
                    isMatch = true;
                }
                else
                {
                    failureMsg = $"Expected DOM / CSS content '{tc.ExpectedOutput}' was not found in browser workspace.";
                }
            }
            else
            {
                isMatch = true;
            }

            if (isMatch) passed++;

            results.Add(new TestCaseResult(
                TestCaseIndex: i + 1,
                Passed: isMatch,
                ActualOutput: isMatch ? tc.ExpectedOutput : null,
                ErrorOutput: failureMsg,
                ExecutionTimeMs: 5.0,
                MemoryUsedKb: 1024,
                FailureReason: failureMsg,
                IsHidden: tc.IsHidden
            ));
        }

        return (passed, results);
    }
}
