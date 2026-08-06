using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Judge.Contracts;
using Platform.Judge.Abstractions;
using Platform.Judge.Models;

namespace Platform.Judge.Engines.Judge0;

public class Judge0ExecutionEngine : IExecutionEngine
{
    private readonly Judge0Client _judge0Client;
    private readonly ITestCaseEvaluator _testCaseEvaluator;
    private readonly ILogger<Judge0ExecutionEngine> _logger;

    public string EngineName => "Judge0Engine";

    public Judge0ExecutionEngine(
        Judge0Client judge0Client,
        ITestCaseEvaluator testCaseEvaluator,
        ILogger<Judge0ExecutionEngine> logger)
    {
        _judge0Client = judge0Client;
        _testCaseEvaluator = testCaseEvaluator;
        _logger = logger;
    }

    public bool CanExecute(string language)
    {
        return Judge0LanguageMapper.GetLanguageId(language).HasValue;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var languageId = Judge0LanguageMapper.GetLanguageId(request.Language);
        if (!languageId.HasValue)
        {
            return new ExecutionResult(
                request.ExecutionId,
                request.SubmissionId,
                ExecutionStatus.Failed,
                Verdict: null,
                InfrastructureFailure.InternalError,
                PassedTestCases: 0,
                TotalTestCases: 0,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: null,
                ExitCode: null,
                ExecutionTimeMs: 0,
                MemoryUsedKb: 0,
                FailureReason: $"Unsupported language '{request.Language}' for Judge0 execution engine.");
        }

        var limits = request.Limits ?? new ExecutionLimits();

        // If specific test cases exist, evaluate them individually or collectively
        var testCases = request.TestCases ?? new List<TestCase>();
        var firstInput = testCases.Count > 0 ? testCases[0].Input : request.StandardInput;
        var firstExpected = testCases.Count > 0 ? testCases[0].ExpectedOutput : null;

        var j0Request = new Judge0SubmissionRequest(
            SourceCode: request.SourceCode,
            LanguageId: languageId.Value,
            Stdin: firstInput,
            ExpectedOutput: firstExpected,
            CpuTimeLimit: limits.CpuTimeLimitSec,
            MemoryLimit: limits.MemoryLimitMb * 1024 // Judge0 expects memory in KB
        );

        var j0Result = await _judge0Client.SubmitSubmissionAsync(j0Request, cancellationToken);
        if (j0Result == null)
        {
            return new ExecutionResult(
                request.ExecutionId,
                request.SubmissionId,
                ExecutionStatus.Failed,
                Verdict: null,
                InfrastructureFailure.ProviderUnavailable,
                PassedTestCases: 0,
                TotalTestCases: testCases.Count,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: null,
                ExitCode: null,
                ExecutionTimeMs: 0,
                MemoryUsedKb: 0,
                FailureReason: "Judge0 execution provider is unavailable or returned an empty response.");
        }

        double timeMs = 0;
        if (!string.IsNullOrEmpty(j0Result.Time) && double.TryParse(j0Result.Time, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedSec))
        {
            timeMs = parsedSec * 1000;
        }

        long memoryKb = j0Result.Memory ?? 0;
        int statusId = j0Result.Status?.Id ?? 0;

        Verdict? verdict = MapStatusIdToVerdict(statusId);
        InfrastructureFailure infraFailure = InfrastructureFailure.None;

        if (!verdict.HasValue && statusId != 3) // StatusId 3 = Accepted in Judge0
        {
            infraFailure = InfrastructureFailure.ExecutionInfrastructureFailure;
        }

        // Evaluate test cases
        var (passedCount, testResults) = _testCaseEvaluator.Evaluate(
            testCases,
            j0Result.Stdout ?? string.Empty,
            j0Result.Stderr ?? j0Result.Message,
            timeMs,
            memoryKb
        );

        if (verdict == Verdict.Accepted && testCases.Count > 0 && passedCount < testCases.Count)
        {
            verdict = Verdict.WrongAnswer;
        }

        return new ExecutionResult(
            request.ExecutionId,
            request.SubmissionId,
            ExecutionStatus.Completed,
            Verdict: verdict,
            InfrastructureFailure: infraFailure,
            PassedTestCases: passedCount,
            TotalTestCases: testCases.Count,
            TestCaseResults: testResults,
            CompilationOutput: j0Result.CompileOutput,
            StandardOutput: j0Result.Stdout,
            StandardError: j0Result.Stderr ?? j0Result.Message,
            ExitCode: j0Result.ExitCode,
            ExecutionTimeMs: timeMs,
            MemoryUsedKb: memoryKb,
            FailureReason: j0Result.Status?.Description
        );
    }

    private static Verdict? MapStatusIdToVerdict(int statusId)
    {
        return statusId switch
        {
            3 => Verdict.Accepted,
            4 => Verdict.WrongAnswer,
            5 => Verdict.TimeLimitExceeded,
            6 => Verdict.CompilationError,
            7 => Verdict.RuntimeError, // SIGSEGV
            8 => Verdict.OutputLimitExceeded, // SIGXFSZ (Output limit)
            9 => Verdict.RuntimeError, // SIGFPE
            10 => Verdict.RuntimeError, // SIGABRT
            11 => Verdict.RuntimeError, // NZEC
            12 => Verdict.RuntimeError, // Other Runtime Error
            14 => Verdict.SecurityViolation, // Exec Format Error
            _ => null
        };
    }
}
