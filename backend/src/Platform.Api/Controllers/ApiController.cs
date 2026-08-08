using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Domain.Results;
using Error = Platform.Domain.Results.Error;
using ErrorType = Platform.Domain.Results.ErrorType;

namespace Platform.Api.Controllers;

/// <summary>
/// Base controller providing shared RFC 7807 error mapping and JWT claim helpers
/// for all API controllers in this service.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiController : ControllerBase
{
    // ── Error Mapping (RFC 7807 ProblemDetails) ───────────────────────────────

    protected IActionResult MapError(Error? error)
    {
        if (error is null)
        {
            return StatusCode(500, new ProblemDetails
            {
                Status = 500,
                Title = "Server.Error",
                Detail = "An unexpected error occurred.",
                Instance = HttpContext.Request.Path
            });
        }

        return error.Type switch
        {
            ErrorType.NotFound             => NotFound(Problem(error, 404)),
            ErrorType.Conflict             => Conflict(Problem(error, 409)),
            ErrorType.Unauthorized         => Unauthorized(Problem(error, 401)),
            ErrorType.Forbidden            => StatusCode(403, Problem(error, 403)),
            ErrorType.Validation           => UnprocessableEntity(Problem(error, 422)),
            ErrorType.SubscriptionRequired => StatusCode(402, Problem(error, 402)),
            ErrorType.Internal             => StatusCode(500, Problem(error, 500)),
            _                              => BadRequest(Problem(error, 400))
        };
    }

    protected ProblemDetails Problem(Error error, int status) => new()
    {
        Status   = status,
        Title    = error.Code,
        Detail   = error.Message,
        Instance = HttpContext.Request.Path
    };

    // ── JWT Claim Helpers ─────────────────────────────────────────────────────

    protected bool TryGetUserId(out long userId)
    {
        userId = 0;
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return raw is not null && long.TryParse(raw, out userId);
    }
}
