using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Assessment.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Exam management — create, update, publish, and list exams.
/// </summary>
[Route("api/exams")]
[Authorize]
public sealed class ExamsController : ApiController
{
    private readonly ISender _sender;
    public ExamsController(ISender sender) => _sender = sender;

    /// <summary>Returns a paged list of exams with filters.</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Exams Paged", Tags = ["Exams"])]
    [ProducesResponseType(typeof(PagedList<ExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams([FromQuery] GetExamsPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns detailed exam information by ID.</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Exam By ID", Tags = ["Exams"])]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetExamByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Creates a new exam (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("exams.manage")]
    [SwaggerOperation(Summary = "Create Exam", Tags = ["Exams"])]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetExamById), new { id = result.Value!.Id }, result.Value)
            : MapError(result.Error);
    }

    /// <summary>Updates an existing exam.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("exams.manage")]
    [SwaggerOperation(Summary = "Update Exam", Tags = ["Exams"])]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExam([FromRoute] long id, [FromBody] UpdateExamRequest request, CancellationToken ct)
    {
        var cmd = new UpdateExamCommand(id, request.Title, request.Description, request.DurationMinutes, request.TotalMarks, request.PassingMarks, request.StartDate, request.EndDate);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Publishes or unpublishes an exam.</summary>
    [HttpPost("{id:long}/publish")]
    [HasPermission("exams.manage")]
    [SwaggerOperation(Summary = "Publish Exam", Tags = ["Exams"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishExam([FromRoute] long id, [FromBody] PublishExamRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new PublishExamCommand(id, request.IsPublished), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }
}

public sealed record UpdateExamRequest(
    string Title,
    string Description,
    int DurationMinutes,
    decimal TotalMarks,
    decimal? PassingMarks,
    System.DateTime? StartDate,
    System.DateTime? EndDate
);

public sealed record PublishExamRequest(bool IsPublished);
