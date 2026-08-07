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
/// Teacher profile — public discovery and authenticated self-update.
/// </summary>
[Route("api/teachers")]
[Authorize]
public sealed class TeacherProfilesController : ApiController
{
    private readonly ISender _sender;
    public TeacherProfilesController(ISender sender) => _sender = sender;

    /// <summary>Returns the public teacher profile for the given user ID.</summary>
    [HttpGet("{userId:long}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Teacher Profile", Tags = ["Teachers"])]
    [ProducesResponseType(typeof(TeacherProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacherProfile([FromRoute] long userId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTeacherProfileQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Updates the authenticated teacher's own profile.</summary>
    [HttpPut("me")]
    [SwaggerOperation(Summary = "Update My Teacher Profile", Tags = ["Teachers"])]
    [ProducesResponseType(typeof(TeacherProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTeacherProfileCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Generates a pre-signed upload URL for the teacher's avatar image (max 5 MB).</summary>
    [HttpPost("me/avatar/upload-url")]
    [SwaggerOperation(Summary = "Upload Teacher Avatar", Tags = ["Teachers"])]
    [ProducesResponseType(typeof(AvatarUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadAvatar([FromBody] UploadUserAvatarCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
