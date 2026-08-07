using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Features.Users.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Student profile — academic details and self-update.
/// </summary>
[Route("api/students")]
[Authorize]
public sealed class StudentProfilesController : ApiController
{
    private readonly ISender _sender;
    public StudentProfilesController(ISender sender) => _sender = sender;

    /// <summary>Returns the student profile for the given user ID.</summary>
    [HttpGet("{userId:long}")]
    [SwaggerOperation(Summary = "Get Student Profile", Tags = ["Students"])]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProfile([FromRoute] long userId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetStudentProfileQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Updates the authenticated student's own academic profile.</summary>
    [HttpPut("me")]
    [SwaggerOperation(Summary = "Update My Student Profile", Tags = ["Students"])]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Generates a pre-signed upload URL for the student's avatar image (max 5 MB).</summary>
    [HttpPost("me/avatar/upload-url")]
    [SwaggerOperation(Summary = "Upload Student Avatar", Tags = ["Students"])]
    [ProducesResponseType(typeof(AvatarUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadAvatar([FromBody] UploadUserAvatarCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
