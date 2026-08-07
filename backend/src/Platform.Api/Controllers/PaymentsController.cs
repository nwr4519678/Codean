using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Commerce.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Student payment history and invoice inspection.
/// </summary>
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ApiController
{
    private readonly ISender _sender;
    public PaymentsController(ISender sender) => _sender = sender;

    /// <summary>Returns paged payment history with invoice details for the logged in student.</summary>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get My Payments", Tags = ["Payments"])]
    [ProducesResponseType(typeof(PagedList<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayments([FromQuery] GetMyPaymentsPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
