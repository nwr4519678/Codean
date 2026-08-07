using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Features.Analytics.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// System dashboard analytics and key performance indicators.
/// </summary>
[Route("api/analytics")]
[Authorize]
public sealed class AnalyticsController : ApiController
{
    private readonly ISender _sender;
    public AnalyticsController(ISender sender) => _sender = sender;

    /// <summary>Returns system-wide aggregated metrics (Admin only).</summary>
    [HttpGet("overview")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Get Platform Overview", Tags = ["Analytics"])]
    [ProducesResponseType(typeof(PlatformOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var result = await _sender.Send(new GetPlatformOverviewQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
