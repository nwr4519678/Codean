using System.Collections.Generic;
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
/// Homework management and student submission grading.
/// </summary>
[Route("api/homeworks")]
[Authorize]
public sealed class HomeworksController : ApiController
{
    private readonly ISender _sender;
    public HomeworksController(ISender sender) => _sender = sender;

    /// <summary>Creates a new homework assignment (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("homeworks.manage")]
    [SwaggerOperation(Summary = "Create Homework", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(HomeworkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateHomework([FromBody] CreateHomeworkCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns published assignments available to students.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List Homeworks", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(PagedList<HomeworkResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHomeworks([FromQuery] GetHomeworksPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns one published assignment for a student.</summary>
    [HttpGet("{id:long}")]
    [SwaggerOperation(Summary = "Get Homework", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(HomeworkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHomework([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetHomeworkByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Submits a student's response to a homework assignment.</summary>
    [HttpPost("{id:long}/submit")]
    [SwaggerOperation(Summary = "Submit Homework", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(HomeworkSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitHomework([FromRoute] long id, [FromBody] SubmitHomeworkRequest request, CancellationToken ct)
    {
        var cmd = new SubmitHomeworkCommand(id, request.SubmissionType, request.FileUrl, request.TextAnswer);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Grades a student homework submission (Teacher or Admin).</summary>
    [HttpPost("submissions/{submissionId:long}/grade")]
    [HasPermission("homeworks.manage")]
    [SwaggerOperation(Summary = "Grade Homework Submission", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(HomeworkSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GradeSubmission([FromRoute] long submissionId, [FromBody] GradeHomeworkSubmissionRequest request, CancellationToken ct)
    {
        var cmd = new GradeHomeworkSubmissionCommand(submissionId, request.Grade, request.Feedback);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns all student submissions for a homework assignment.</summary>
    [HttpGet("{id:long}/submissions")]
    [HasPermission("homeworks.manage")]
    [SwaggerOperation(Summary = "Get Homework Submissions", Tags = ["Homeworks"])]
    [ProducesResponseType(typeof(IReadOnlyList<HomeworkSubmissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissions([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetHomeworkSubmissionsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}

public sealed record SubmitHomeworkRequest(
    string SubmissionType,
    string FileUrl,
    string TextAnswer
);

public sealed record GradeHomeworkSubmissionRequest(
    decimal Grade,
    string Feedback
);
