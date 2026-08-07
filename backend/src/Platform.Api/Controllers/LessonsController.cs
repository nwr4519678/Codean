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
/// Lesson management — create, update, publish, delete, attach resources.
/// </summary>
[Route("api/lessons")]
[Authorize]
public sealed class LessonsController : ApiController
{
    private readonly ISender _sender;
    public LessonsController(ISender sender) => _sender = sender;

    /// <summary>Creates a new lesson within a module.</summary>
    [HttpPost]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Create Lesson", Tags = ["Lessons"])]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Updates a lesson.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Update Lesson", Tags = ["Lessons"])]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLesson([FromRoute] long id, [FromBody] UpdateLessonRequest request, CancellationToken ct)
    {
        var cmd = new UpdateLessonCommand(id, request.Title, request.Description, request.Duration, request.Order, request.IsPublished);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Publishes or unpublishes a lesson.</summary>
    [HttpPost("{id:long}/publish")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Publish Lesson", Tags = ["Lessons"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishLesson([FromRoute] long id, [FromBody] PublishLessonRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new PublishLessonCommand(id, request.IsPublished), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Deletes a lesson.</summary>
    [HttpDelete("{id:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Delete Lesson", Tags = ["Lessons"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLesson([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteLessonCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Attaches a PDF or document resource to a lesson.</summary>
    [HttpPost("{id:long}/resources")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Attach Lesson Resource", Tags = ["Lessons"])]
    [ProducesResponseType(typeof(LessonResourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttachResource([FromRoute] long id, [FromBody] AttachResourceRequest request, CancellationToken ct)
    {
        var cmd = new AttachLessonResourceCommand(id, request.ResourceType, request.FileUrl, request.FileName, request.FileSizeBytes);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Removes a resource attachment from a lesson.</summary>
    [HttpDelete("resources/{resourceId:long}")]
    [HasPermission("courses.manage")]
    [SwaggerOperation(Summary = "Remove Lesson Resource", Tags = ["Lessons"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveResource([FromRoute] long resourceId, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveLessonResourceCommand(resourceId), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }
}

public sealed record UpdateLessonRequest(
    string Title,
    string Description,
    int? Duration,
    int Order,
    bool IsPublished
);

public sealed record PublishLessonRequest(bool IsPublished);

public sealed record AttachResourceRequest(
    string ResourceType,
    string FileUrl,
    string FileName,
    long? FileSizeBytes
);
