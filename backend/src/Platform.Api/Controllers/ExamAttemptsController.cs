using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Features.Assessment.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Student exam execution — start attempt, submit answers, get attempt status.
/// </summary>
[Route("api/exam-attempts")]
[Authorize]
public sealed class ExamAttemptsController : ApiController
{
    private readonly ISender _sender;
    public ExamAttemptsController(ISender sender) => _sender = sender;

    /// <summary>Starts a new timed attempt for a published exam.</summary>
    [HttpPost("start/{examId:long}")]
    [SwaggerOperation(Summary = "Start Exam Attempt", Tags = ["Exam Attempts"])]
    [ProducesResponseType(typeof(ExamAttemptResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartAttempt([FromRoute] long examId, CancellationToken ct)
    {
        var result = await _sender.Send(new StartExamAttemptCommand(examId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Submits student answers for an exam attempt.</summary>
    [HttpPost("{id:long}/submit")]
    [SwaggerOperation(Summary = "Submit Exam Attempt", Tags = ["Exam Attempts"])]
    [ProducesResponseType(typeof(ExamAttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAttempt([FromRoute] long id, [FromBody] SubmitExamAttemptCommand cmd, CancellationToken ct)
    {
        if (id != cmd.AttemptId) return BadRequest();
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns attempt status and scores by attempt ID.</summary>
    [HttpGet("{id:long}")]
    [SwaggerOperation(Summary = "Get Exam Attempt By ID", Tags = ["Exam Attempts"])]
    [ProducesResponseType(typeof(ExamAttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttemptById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetExamAttemptByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
