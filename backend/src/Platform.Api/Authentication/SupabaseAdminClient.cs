using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Platform.Infrastructure.Configuration;

namespace Platform.Api.Authentication;

public sealed class SupabaseAdminClient
{
    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;

    public SupabaseAdminClient(HttpClient http, IOptions<SupabaseOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<bool> CreateUserAsync(string email, string password, string fullName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.Url) || string.IsNullOrWhiteSpace(_options.ServiceRoleKey))
            throw new InvalidOperationException("Supabase:ServiceRoleKey is required for admin teacher creation.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Url.TrimEnd('/')}/auth/v1/admin/users")
        {
            Content = JsonContent.Create(new
            {
                email,
                password,
                email_confirm = true,
                user_metadata = new { full_name = fullName }
            })
        };
        request.Headers.Add("apikey", _options.ServiceRoleKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);

        using var response = await _http.SendAsync(request, ct);
        if (response.IsSuccessStatusCode) return true;
        var detail = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException($"Supabase could not create the teacher account ({(int)response.StatusCode}): {detail}");
    }
}
