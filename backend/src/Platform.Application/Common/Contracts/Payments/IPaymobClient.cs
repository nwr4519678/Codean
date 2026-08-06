using Platform.Domain.Results;

namespace Platform.Application.Common.Contracts.Payments;

public sealed record PaymobIntention(
    string ClientSecret,
    string CheckoutUrl,
    string PaymobOrderId,
    decimal Amount,
    string Currency);

public sealed record PaymobWebhookPayload(
    string TransactionId,
    bool Success,
    string? OrderId,
    string? Method,
    decimal Amount,
    string Currency,
    string RawJson);

public interface IPaymobClient
{
    /// <summary>Creates a payment intention and returns the hosted-checkout URL.</summary>
    Task<Result<PaymobIntention>> CreateIntentionAsync(
        string merchantOrderId,
        decimal amount,
        string currency,
        IEnumerable<(string name, int qty, decimal amountCents)> items,
        long userId,
        string? couponCode = null,
        CancellationToken ct = default);

    /// <summary>Validates the HMAC signature on an incoming webhook.</summary>
    bool ValidateHmac(string rawBody, string signature, string? hmacSecret = null);

    /// <summary>Verifies a transaction with the Paymob transaction API (defence in depth).</summary>
    Task<bool> VerifyTransactionAsync(string transactionId, CancellationToken ct = default);
}
