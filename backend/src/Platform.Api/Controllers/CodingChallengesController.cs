using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Features.Judge.Commands;
using Platform.Application.Features.Judge.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Coding challenge management and async judge-backed code submission.
///
/// Submission lifecycle:
///   POST /api/challenges/{id}/submit  → returns 202 + ExecutionId
///   GET  /api/submissions/{id}/status → poll until Status = Completed | Failed
/// </summary>
[Route("api/challenges")]
[Authorize]
public sealed class CodingChallengesController : ApiController
{
    private readonly ISender _sender;
    public CodingChallengesController(ISender sender) => _sender = sender;

    // ── Challenge Management ──────────────────────────────────────────────

    /// <summary>Returns all coding challenges available for practice.</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "List Coding Challenges", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(IReadOnlyList<CodingChallengeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChallenges(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCodingChallengesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns challenge details including starter code (test cases hidden).</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Coding Challenge", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(CodingChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChallenge([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCodingChallengeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Creates a new coding challenge (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("challenges.manage")]
    [SwaggerOperation(Summary = "Create Coding Challenge", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(CodingChallengeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateChallenge([FromBody] CreateCodingChallengeCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetChallenge), new { id = result.Value!.Id }, result.Value)
            : MapError(result.Error);
    }

    /// <summary>Updates an existing coding challenge.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("challenges.manage")]
    [SwaggerOperation(Summary = "Update Coding Challenge", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(CodingChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChallenge(
        [FromRoute] long id,
        [FromBody] UpdateChallengeRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateCodingChallengeCommand(
            id, request.Title, request.Description,
            request.StarterCode, request.TestCases,
            request.Difficulty, request.Marks);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    // ── Async Submission ──────────────────────────────────────────────────

    /// <summary>
    /// Submits student code to the Judge Engine asynchronously.
    /// Returns 202 Accepted immediately — client polls GET /api/submissions/{id}/status.
    /// </summary>
    [HttpPost("{id:long}/submit")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("code_submission")]
    [SwaggerOperation(Summary = "Submit Code", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(SubmitCodeResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitCode(
        [FromRoute] long id,
        [FromBody] SubmitCodeRequest request,
        CancellationToken ct)
    {
        var cmd = new SubmitCodeChallengeCommand(id, request.SourceCode, request.Language);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? Accepted($"/api/submissions/{result.Value!.SubmissionId}/status", result.Value)
            : MapError(result.Error);
    }

    /// <summary>Returns all submissions made by the current student for a challenge.</summary>
    [HttpGet("{id:long}/my-submissions")]
    [SwaggerOperation(Summary = "Get My Submissions", Tags = ["Coding Challenges"])]
    [ProducesResponseType(typeof(IReadOnlyList<CodeSubmissionResultResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySubmissions([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMySubmissionsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}

/// <summary>
/// Submission status polling — decoupled from challenge routing.
/// </summary>
[Route("api/submissions")]
[Authorize]
public sealed class SubmissionsController : ApiController
{
    private readonly ISender _sender;
    public SubmissionsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Polls the judge result for a given submission ID.
    /// Status transitions: Queued → Running → Completed | Failed.
    /// </summary>
    [HttpGet("{submissionId:long}/status")]
    [SwaggerOperation(Summary = "Get Submission Status", Tags = ["Submissions"])]
    [ProducesResponseType(typeof(CodeSubmissionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmissionStatus([FromRoute] long submissionId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSubmissionStatusQuery(submissionId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────

public sealed record SubmitCodeRequest(string SourceCode, string Language);

public sealed record UpdateChallengeRequest(
    string Title,
    string Description,
    string StarterCode,
    string TestCases,
    string Difficulty,
    decimal Marks
);
