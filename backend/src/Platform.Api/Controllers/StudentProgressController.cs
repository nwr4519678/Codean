using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Features.Learning.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Student progress tracking — record lesson watch time/completion and retrieve course progress.
/// </summary>
[Route("api/progress")]
[Authorize]
public sealed class StudentProgressController : ApiController
{
    private readonly ISender _sender;
    public StudentProgressController(ISender sender) => _sender = sender;

    /// <summary>Records or updates student progress on a lesson (completion % & watch time).</summary>
    [HttpPost("track")]
    [SwaggerOperation(Summary = "Track Lesson Progress", Tags = ["Student Progress"])]
    [ProducesResponseType(typeof(StudentProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TrackProgress([FromBody] TrackLessonProgressCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns overall student progress across all lessons in a course.</summary>
    [HttpGet("courses/{courseId:long}")]
    [SwaggerOperation(Summary = "Get Course Progress", Tags = ["Student Progress"])]
    [ProducesResponseType(typeof(CourseProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseProgress([FromRoute] long courseId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCourseProgressQuery(courseId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
