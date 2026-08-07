using System.IO;
using System.Text;
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
/// Paymob webhook callback receiver endpoint.
/// Must allow anonymous access — protected strictly via HMAC signature verification.
/// </summary>
[Route("api/webhooks/paymob")]
[AllowAnonymous]
public sealed class PaymobWebhookController : ApiController
{
    private readonly ISender _sender;
    public PaymobWebhookController(ISender sender) => _sender = sender;

    /// <summary>Receives Paymob transaction notifications, validates HMAC, processes idempotently.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Paymob Webhook Callback", Tags = ["Webhooks"])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessWebhook([FromBody] PaymobWebhookCallbackRequest request, CancellationToken ct)
    {
        // Read raw body for HMAC verification
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["X-Paymob-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            signature = Request.Query["hmac"].ToString();
        }

        var cmd = new ProcessPaymobWebhookCommand(
            rawBody,
            signature,
            request.TransactionId ?? request.Id ?? string.Empty,
            request.Success,
            request.OrderId,
            request.PaymentMethod,
            request.Amount,
            request.Currency ?? "EGP"
        );

        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(new { status = "received" }) : MapError(result.Error);
    }
}

public sealed record PaymobWebhookCallbackRequest(
    string? TransactionId,
    string? Id,
    bool Success,
    string? OrderId,
    string? PaymentMethod,
    decimal Amount,
    string? Currency
);
