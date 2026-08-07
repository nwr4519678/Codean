using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Learning.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Course management — creation, updating, publishing, and public discovery.
/// </summary>
[Route("api/courses")]
[Authorize]
public sealed class CoursesController : ApiController
{
    private readonly ISender _sender;
    public CoursesController(ISender sender) => _sender = sender;

    /// <summary>Returns a paged list of courses with filtering.</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Courses Paged", Tags = ["Courses"])]
    [ProducesResponseType(typeof(PagedList<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses([FromQuery] GetCoursesPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns detailed course information including modules and lessons.</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Course By ID", Tags = ["Courses"])]
    [ProducesResponseType(typeof(CourseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCourseByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Creates a new course (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Create Course", Tags = ["Courses"])]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCourseById), new { id = result.Value!.Id }, result.Value)
            : MapError(result.Error);
    }

    /// <summary>Updates an existing course.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Update Course", Tags = ["Courses"])]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse([FromRoute] long id, [FromBody] UpdateCourseRequest request, CancellationToken ct)
    {
        var cmd = new UpdateCourseCommand(id, request.Title, request.Description, request.Thumbnail, request.Category, request.Price);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Publishes a course making it accessible to students.</summary>
    [HttpPost("{id:long}/publish")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Publish Course", Tags = ["Courses"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishCourse([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new PublishCourseCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Archives a course making it hidden.</summary>
    [HttpPost("{id:long}/archive")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Archive Course", Tags = ["Courses"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveCourse([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new ArchiveCourseCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }
}

public sealed record UpdateCourseRequest(
    string Title,
    string Description,
    string Thumbnail,
    string Category,
    decimal Price
);
