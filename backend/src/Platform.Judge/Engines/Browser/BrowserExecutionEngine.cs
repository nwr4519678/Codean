using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Platform.Judge.Contracts;
using Platform.Judge.Abstractions;
using Platform.Judge.Models;

namespace Platform.Judge.Engines.Browser;

public class BrowserExecutionEngine : IExecutionEngine
{
    private readonly BrowserTestRunner _testRunner;

    public string EngineName => "BrowserEngine";

    public BrowserExecutionEngine(BrowserTestRunner testRunner)
    {
        _testRunner = testRunner;
    }

    public bool CanExecute(string language)
    {
        return string.Equals(language, "html", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(language, "css", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(language, "browserjs", StringComparison.OrdinalIgnoreCase);
    }

    public Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var testCases = request.TestCases ?? new List<TestCase>();
        var (passedCount, results) = _testRunner.EvaluateBrowserAssertions(
            htmlContent: string.Equals(request.Language, "html", StringComparison.OrdinalIgnoreCase) ? request.SourceCode : string.Empty,
            cssContent: string.Equals(request.Language, "css", StringComparison.OrdinalIgnoreCase) ? request.SourceCode : string.Empty,
            jsContent: string.Equals(request.Language, "browserjs", StringComparison.OrdinalIgnoreCase) ? request.SourceCode : string.Empty,
            assertions: testCases
        );

        var verdict = (passedCount == testCases.Count || testCases.Count == 0) ? Verdict.Accepted : Verdict.WrongAnswer;

        var result = new ExecutionResult(
            ExecutionId: request.ExecutionId,
            SubmissionId: request.SubmissionId,
            Status: ExecutionStatus.Completed,
            Verdict: verdict,
            InfrastructureFailure: InfrastructureFailure.None,
            PassedTestCases: passedCount,
            TotalTestCases: testCases.Count,
            TestCaseResults: results,
            CompilationOutput: null,
            StandardOutput: "Browser DOM evaluation completed successfully.",
            StandardError: null,
            ExitCode: 0,
            ExecutionTimeMs: 10.0,
            MemoryUsedKb: 2048,
            FailureReason: verdict == Verdict.Accepted ? null : "DOM/CSS assertion checks failed."
        );

        return Task.FromResult(result);
    }
}
