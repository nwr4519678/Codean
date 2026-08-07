using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Features.Learning.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Course module management — create, update, reorder, delete.
/// </summary>
[Route("api/modules")]
[Authorize]
public sealed class CourseModulesController : ApiController
{
    private readonly ISender _sender;
    public CourseModulesController(ISender sender) => _sender = sender;

    /// <summary>Creates a new module within a course.</summary>
    [HttpPost]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Create Course Module", Tags = ["Course Modules"])]
    [ProducesResponseType(typeof(CourseModuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateModule([FromBody] CreateCourseModuleCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Updates a course module.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Update Course Module", Tags = ["Course Modules"])]
    [ProducesResponseType(typeof(CourseModuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateModule([FromRoute] long id, [FromBody] UpdateCourseModuleRequest request, CancellationToken ct)
    {
        var cmd = new UpdateCourseModuleCommand(id, request.Title, request.MonthNumber, request.Order, request.Description);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Deletes a course module.</summary>
    [HttpDelete("{id:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Delete Course Module", Tags = ["Course Modules"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModule([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteCourseModuleCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }
}

public sealed record UpdateCourseModuleRequest(
    string Title,
    int MonthNumber,
    int Order,
    string Description
);
