using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Communication.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Announcement broadcasting and feed retrieval.
/// </summary>
[Route("api/announcements")]
[Authorize]
public sealed class AnnouncementsController : ApiController
{
    private readonly ISender _sender;
    public AnnouncementsController(ISender sender) => _sender = sender;

    /// <summary>Returns paged announcements filtered by course or teacher (pinned items first).</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Announcements Paged", Tags = ["Announcements"])]
    [ProducesResponseType(typeof(PagedList<AnnouncementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnouncements([FromQuery] GetAnnouncementsPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Creates a new announcement (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("announcements.manage")]
    [SwaggerOperation(Summary = "Create Announcement", Tags = ["Announcements"])]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Updates an existing announcement.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("announcements.manage")]
    [SwaggerOperation(Summary = "Update Announcement", Tags = ["Announcements"])]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAnnouncement(
        [FromRoute] long id,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateAnnouncementCommand(id, request.Title, request.Body, request.IsPinned);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Deletes an announcement.</summary>
    [HttpDelete("{id:long}")]
    [HasPermission("announcements.manage")]
    [SwaggerOperation(Summary = "Delete Announcement", Tags = ["Announcements"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAnnouncement([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteAnnouncementCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }
}

public sealed record UpdateAnnouncementRequest(
    string Title,
    string Body,
    bool IsPinned
);
