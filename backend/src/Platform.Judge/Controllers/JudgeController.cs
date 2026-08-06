using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Judge.Contracts;
using Platform.Judge.Models;
using Platform.Judge.Services;
using Platform.Judge.Worker;

namespace Platform.Judge.Controllers;

[ApiController]
[Route("api/judge/executions")]
public class JudgeController : ControllerBase
{
    private readonly ExecutionQueue _queue;
    private readonly ExecutionStore _store;

    public JudgeController(ExecutionQueue queue, ExecutionStore store)
    {
        _queue = queue;
        _store = store;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExecutionSubmissionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitExecution([FromBody] CodeExecutionRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ExecutionId) || string.IsNullOrWhiteSpace(request.Language))
        {
            return BadRequest("Invalid execution request. ExecutionId and Language are required.");
        }

        // Idempotency check: If ExecutionId already exists, return existing status response
        bool isNew = _store.TryAddOrGetSubmission(request.ExecutionId, request.SubmissionId, out var statusResponse);
        if (!isNew)
        {
            return AcceptedAtAction(nameof(GetStatus), new { executionId = request.ExecutionId }, statusResponse);
        }

        // Map DTO to internal ExecutionRequest
        var internalLimits = request.Limits != null
            ? new ExecutionLimits(request.Limits.CpuTimeLimitSec, request.Limits.MemoryLimitMb, request.Limits.OutputSizeLimitKb)
            : new ExecutionLimits();

        var internalTestCases = request.TestCases?.Select(tc => new TestCase(tc.Input, tc.ExpectedOutput, tc.IsHidden)).ToList() ?? new List<TestCase>();

        var internalRequest = new ExecutionRequest(
            request.ExecutionId,
            request.SubmissionId,
            request.Language,
            request.SourceCode,
            request.StandardInput,
            internalTestCases,
            internalLimits
        );

        // Enqueue job for background processing
        await _queue.EnqueueAsync(internalRequest, cancellationToken);

        return AcceptedAtAction(nameof(GetStatus), new { executionId = request.ExecutionId }, statusResponse);
    }

    [HttpGet("{executionId}/status")]
    [ProducesResponseType(typeof(ExecutionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetStatus([FromRoute] string executionId)
    {
        var status = _store.GetStatus(executionId);
        if (status == null)
        {
            return NotFound($"Execution '{executionId}' not found.");
        }

        return Ok(status);
    }

    [HttpGet("{executionId}/result")]
    [ProducesResponseType(typeof(CodeExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetResult([FromRoute] string executionId)
    {
        var internalResult = _store.GetResult(executionId);
        if (internalResult == null)
        {
            return NotFound($"Execution result for '{executionId}' is not yet available or completed.");
        }

        var dtoTestResults = internalResult.TestCaseResults.Select(r => new TestCaseResultDto(
            r.TestCaseIndex,
            r.Passed,
            r.ActualOutput,
            r.ErrorOutput,
            r.ExecutionTimeMs,
            r.MemoryUsedKb,
            r.FailureReason,
            r.IsHidden
        )).ToList();

        var dtoResult = new CodeExecutionResult(
            internalResult.ExecutionId,
            internalResult.SubmissionId,
            internalResult.Status,
            internalResult.Verdict,
            internalResult.InfrastructureFailure,
            internalResult.PassedTestCases,
            internalResult.TotalTestCases,
            dtoTestResults,
            internalResult.CompilationOutput,
            internalResult.StandardOutput,
            internalResult.StandardError,
            internalResult.ExitCode,
            internalResult.ExecutionTimeMs,
            internalResult.MemoryUsedKb,
            internalResult.FailureReason
        );

        return Ok(dtoResult);
    }
}
