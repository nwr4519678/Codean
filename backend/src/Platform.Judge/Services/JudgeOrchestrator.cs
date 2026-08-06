using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Judge.Contracts;
using Platform.Judge.Abstractions;
using Platform.Judge.Models;

namespace Platform.Judge.Services;

public class JudgeOrchestrator : IExecutionOrchestrator
{
    private readonly ILanguageResolver _languageResolver;
    private readonly IEnumerable<IExecutionEngine> _executionEngines;
    private readonly ILogger<JudgeOrchestrator> _logger;

    public JudgeOrchestrator(
        ILanguageResolver languageResolver,
        IEnumerable<IExecutionEngine> executionEngines,
        ILogger<JudgeOrchestrator> logger)
    {
        _languageResolver = languageResolver;
        _executionEngines = executionEngines;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting execution orchestrator for ExecutionId {ExecutionId}, Language {Language}",
            request.ExecutionId, request.Language);

        if (!_languageResolver.IsSupported(request.Language))
        {
            _logger.LogWarning("ExecutionId {ExecutionId} failed: Unsupported language '{Language}'", request.ExecutionId, request.Language);
            return new ExecutionResult(
                ExecutionId: request.ExecutionId,
                SubmissionId: request.SubmissionId,
                Status: ExecutionStatus.Failed,
                Verdict: null,
                InfrastructureFailure: InfrastructureFailure.InternalError,
                PassedTestCases: 0,
                TotalTestCases: request.TestCases?.Count ?? 0,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: null,
                ExitCode: null,
                ExecutionTimeMs: 0,
                MemoryUsedKb: 0,
                FailureReason: $"Language '{request.Language}' is not supported by the platform."
            );
        }

        var engine = _executionEngines.FirstOrDefault(e => e.CanExecute(request.Language));
        if (engine == null)
        {
            _logger.LogError("No execution engine registered for language '{Language}' for ExecutionId {ExecutionId}",
                request.Language, request.ExecutionId);

            return new ExecutionResult(
                ExecutionId: request.ExecutionId,
                SubmissionId: request.SubmissionId,
                Status: ExecutionStatus.Failed,
                Verdict: null,
                InfrastructureFailure: InfrastructureFailure.ExecutionInfrastructureFailure,
                PassedTestCases: 0,
                TotalTestCases: request.TestCases?.Count ?? 0,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: null,
                ExitCode: null,
                ExecutionTimeMs: 0,
                MemoryUsedKb: 0,
                FailureReason: $"No active execution engine available for language '{request.Language}'."
            );
        }

        try
        {
            _logger.LogInformation("Executing ExecutionId {ExecutionId} using engine {EngineName}",
                request.ExecutionId, engine.EngineName);

            var result = await engine.ExecuteAsync(request, cancellationToken);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ExecutionId {ExecutionId} was cancelled or timed out", request.ExecutionId);
            return new ExecutionResult(
                ExecutionId: request.ExecutionId,
                SubmissionId: request.SubmissionId,
                Status: ExecutionStatus.Cancelled,
                Verdict: Verdict.TimeLimitExceeded,
                InfrastructureFailure: InfrastructureFailure.None,
                PassedTestCases: 0,
                TotalTestCases: request.TestCases?.Count ?? 0,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: "Execution cancelled or wall-clock timeout exceeded.",
                ExitCode: null,
                ExecutionTimeMs: (request.Limits?.CpuTimeLimitSec ?? 2.0) * 1000,
                MemoryUsedKb: 0,
                FailureReason: "Execution exceeded allowed execution time window."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during execution for ExecutionId {ExecutionId}", request.ExecutionId);
            return new ExecutionResult(
                ExecutionId: request.ExecutionId,
                SubmissionId: request.SubmissionId,
                Status: ExecutionStatus.Failed,
                Verdict: null,
                InfrastructureFailure: InfrastructureFailure.InternalError,
                PassedTestCases: 0,
                TotalTestCases: request.TestCases?.Count ?? 0,
                TestCaseResults: new List<TestCaseResult>(),
                CompilationOutput: null,
                StandardOutput: null,
                StandardError: ex.Message,
                ExitCode: null,
                ExecutionTimeMs: 0,
                MemoryUsedKb: 0,
                FailureReason: "An unexpected internal error occurred during code execution."
            );
        }
    }
}
