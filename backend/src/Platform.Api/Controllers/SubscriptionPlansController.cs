using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Features.Commerce.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Subscription plan administration and public catalog discovery.
/// </summary>
[Route("api/plans")]
[Authorize]
public sealed class SubscriptionPlansController : ApiController
{
    private readonly ISender _sender;
    public SubscriptionPlansController(ISender sender) => _sender = sender;

    /// <summary>Returns active subscription plans for public display.</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Subscription Plans", Tags = ["Subscription Plans"])]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans([FromQuery] GetSubscriptionPlansQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns a subscription plan by ID.</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get Subscription Plan By ID", Tags = ["Subscription Plans"])]
    [ProducesResponseType(typeof(SubscriptionPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSubscriptionPlanByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Creates a new subscription plan (Teacher or Admin).</summary>
    [HttpPost]
    [HasPermission("plans.manage")]
    [SwaggerOperation(Summary = "Create Subscription Plan", Tags = ["Subscription Plans"])]
    [ProducesResponseType(typeof(SubscriptionPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPlanById), new { id = result.Value!.Id }, result.Value)
            : MapError(result.Error);
    }

    /// <summary>Updates an existing subscription plan.</summary>
    [HttpPut("{id:long}")]
    [HasPermission("plans.manage")]
    [SwaggerOperation(Summary = "Update Subscription Plan", Tags = ["Subscription Plans"])]
    [ProducesResponseType(typeof(SubscriptionPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlan(
        [FromRoute] long id,
        [FromBody] UpdateSubscriptionPlanRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateSubscriptionPlanCommand(id, request.Name, request.Price, request.Description, request.IsActive);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
}

public sealed record UpdateSubscriptionPlanRequest(
    string Name,
    decimal Price,
    string Description,
    bool IsActive
);
