using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Analytics.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// System security &amp; audit trail search for compliance (Admin only).
/// </summary>
[Route("api/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ApiController
{
    private readonly ISender _sender;
    public AuditLogsController(ISender sender) => _sender = sender;

    /// <summary>Returns paged system audit logs with filters for user, action, and entity type.</summary>
    [HttpGet]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Get Audit Logs Paged", Tags = ["Audit Logs"])]
    [ProducesResponseType(typeof(PagedList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
