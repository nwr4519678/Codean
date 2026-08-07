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
/// In-app notification center for authenticated users.
/// </summary>
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ApiController
{
    private readonly ISender _sender;
    public NotificationsController(ISender sender) => _sender = sender;

    /// <summary>Returns paged notifications for the logged in user.</summary>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get My Notifications", Tags = ["Notifications"])]
    [ProducesResponseType(typeof(PagedList<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] GetMyNotificationsPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns the total number of unread notifications for the badge counter.</summary>
    [HttpGet("me/unread-count")]
    [SwaggerOperation(Summary = "Get Unread Count", Tags = ["Notifications"])]
    [ProducesResponseType(typeof(GetUnreadCountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await _sender.Send(new GetUnreadNotificationCountQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Marks a specific notification as read.</summary>
    [HttpPost("{id:long}/read")]
    [SwaggerOperation(Summary = "Mark Notification Read", Tags = ["Notifications"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new MarkNotificationAsReadCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Marks all unread notifications as read in bulk.</summary>
    [HttpPost("me/read-all")]
    [SwaggerOperation(Summary = "Mark All Notifications Read", Tags = ["Notifications"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var result = await _sender.Send(new MarkAllNotificationsAsReadCommand(), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Sends a system notification to a target user (Admin only).</summary>
    [HttpPost("send")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Send System Notification", Tags = ["Notifications"])]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
