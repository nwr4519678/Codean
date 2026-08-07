using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Features.Commerce.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Student subscription management and checkout initiation.
/// </summary>
[Route("api/subscriptions")]
[Authorize]
public sealed class SubscriptionsController : ApiController
{
    private readonly ISender _sender;
    public SubscriptionsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Initiates a Paymob checkout session for the specified subscription plan.
    /// Returns 200 OK with the Paymob Hosted Checkout URL.
    /// </summary>
    [HttpPost("checkout")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("payment_checkout")]
    [SwaggerOperation(Summary = "Initiate Subscription Checkout", Tags = ["Subscriptions"])]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiateCheckout([FromBody] InitiateCheckoutCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns active subscriptions owned by the current student.</summary>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get My Subscriptions", Tags = ["Subscriptions"])]
    [ProducesResponseType(typeof(IReadOnlyList<StudentSubscriptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySubscriptions(CancellationToken ct)
    {
        var result = await _sender.Send(new GetMySubscriptionsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}
