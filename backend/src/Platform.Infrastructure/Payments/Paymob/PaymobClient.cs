using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.Payments;
using Platform.Domain.Results;

namespace Platform.Infrastructure.Payments.Paymob;

public sealed class PaymobOptions
{
    public string ApiKey { get; set; } = "";
    public string HmacSecret { get; set; } = "";
    public int IntegrationIdCards { get; set; }
    public int IntegrationIdMobileWallets { get; set; }
    public int IframeId { get; set; }
    public string BaseUrl { get; set; } = "https://accept.paymob.com";
}

/// <summary>
/// Anti-corruption-layer adapter for the Paymob payment gateway.
/// Handles intention creation, HMAC validation, and transaction verification.
/// </summary>
public sealed class PaymobClient : IPaymobClient
{
    private readonly HttpClient _http;
    private readonly PaymobOptions _opt;
    private readonly ILogger<PaymobClient> _logger;

    public PaymobClient(HttpClient http, IOptions<PaymobOptions> opt, ILogger<PaymobClient> logger)
    {
        _http = http;
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task<Result<PaymobIntention>> CreateIntentionAsync(
        string merchantOrderId, decimal amount, string currency,
        IEnumerable<(string name, int qty, decimal amountCents)> items,
        long userId, string? couponCode = null, CancellationToken ct = default)
    {
        try
        {
            // Paymob expects amount in major units (e.g. EGP). Currency = "EGP".
            var payload = new
            {
                amount = (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero), // piasters/cents
                currency = currency.ToUpperInvariant(),
                payment_methods = new[] { _opt.IntegrationIdCards, _opt.IntegrationIdMobileWallets },
                billing_data = new
                {
                    first_name = "user",
                    last_name = "user",
                    email = $"{userId}@platform.local",
                    phone_number = "+201000000000",
                    country = "EG",
                    city = "Cairo",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    state = "NA"
                },
                extras = new { merchant_order_id = merchantOrderId, coupon_code = couponCode ?? "" },
                special_reference = merchantOrderId
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"{_opt.BaseUrl}/v1/intention/")
            {
                Content = JsonContent.Create(payload)
            };
            req.Headers.Add("Authorization", $"Token {_opt.ApiKey}");

            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogError("Paymob intention failed: {Status} {Body}", resp.StatusCode, body);
                return Error.Provider("paymob.intention_failed", "Payment provider rejected the request.");
            }

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            var clientSecret = json.GetProperty("client_secret").GetString()!;
            var checkoutUrl = $"{json.GetProperty("intention_detail").GetProperty("redirection_url").GetString()}?client_secret={clientSecret}";
            var paymobOrderId = json.GetProperty("id").GetInt64().ToString();

            return new PaymobIntention(clientSecret, checkoutUrl, paymobOrderId, amount, currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob intention exception");
            return Error.Provider("paymob.unavailable", "Payment provider is unavailable.");
        }
    }

    public bool ValidateHmac(string rawBody, string signature, string? hmacSecret = null)
    {
        var secret = hmacSecret ?? _opt.HmacSecret;
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature)) return false;
        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var provided = Convert.FromHexString(signature);
            return CryptographicOperations.FixedTimeEquals(computed, provided);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Paymob HMAC validation failed");
            return false;
        }
    }

    public async Task<bool> VerifyTransactionAsync(string transactionId, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{_opt.BaseUrl}/api/acceptance/transactions/{transactionId}");
            req.Headers.Add("Authorization", $"Token {_opt.ApiKey}");
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            return json.TryGetProperty("success", out var ok) && ok.GetBoolean();
        }
        catch
        {
            return false;
        }
    }
}
