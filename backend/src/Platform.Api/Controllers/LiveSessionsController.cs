using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Features.LiveSessions.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Live virtual classroom scheduling (Google Meet / Microsoft Teams), attendance and recordings.
/// </summary>
[Route("api/live-sessions")]
[Authorize]
public sealed class LiveSessionsController : ApiController
{
    private readonly ISender _sender;
    public LiveSessionsController(ISender sender) => _sender = sender;

    // ── Session Management ────────────────────────────────────────────────

    /// <summary>Returns all live sessions owned by the authenticated teacher.</summary>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get My Live Sessions", Tags = ["Live Sessions"])]
    [ProducesResponseType(typeof(IReadOnlyList<LiveSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySessions([FromQuery] GetLiveSessionsByTeacherQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Schedules a new live session on Google Meet or Microsoft Teams.</summary>
    [HttpPost]
    [HasPermission("live-sessions.manage")]
    [SwaggerOperation(Summary = "Schedule Live Session", Tags = ["Live Sessions"])]
    [ProducesResponseType(typeof(LiveSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ScheduleSession([FromBody] ScheduleLiveSessionCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Cancels a scheduled or live session and best-effort deletes provider meeting.</summary>
    [HttpPost("{id:long}/cancel")]
    [HasPermission("live-sessions.manage")]
    [SwaggerOperation(Summary = "Cancel Live Session", Tags = ["Live Sessions"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSession([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelLiveSessionCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Attendance ────────────────────────────────────────────────────────

    /// <summary>Records student joining a live session (idempotent check-in).</summary>
    [HttpPost("{id:long}/attend")]
    [SwaggerOperation(Summary = "Record Attendance", Tags = ["Live Sessions"])]
    [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAttendance([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new RecordAttendanceCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns full attendance roster for a session (Teacher/Admin).</summary>
    [HttpGet("{id:long}/attendance")]
    [HasPermission("live-sessions.manage")]
    [SwaggerOperation(Summary = "Get Session Attendance", Tags = ["Live Sessions"])]
    [ProducesResponseType(typeof(IReadOnlyList<AttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSessionAttendanceQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
