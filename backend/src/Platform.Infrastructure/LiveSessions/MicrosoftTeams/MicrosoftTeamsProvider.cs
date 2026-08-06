using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Platform.Application.Common.Contracts.LiveSessions;
using Platform.Domain.Results;

namespace Platform.Infrastructure.LiveSessions.MicrosoftTeams;

public sealed class MicrosoftTeamsOptions
{
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string OrganizerUserId { get; set; } = ""; // UPN of the organizer
}

public sealed class MicrosoftTeamsProvider : IMicrosoftTeamsProvider
{
    private readonly HttpClient _http;
    private readonly MicrosoftTeamsOptions _opt;
    private readonly ILogger<MicrosoftTeamsProvider> _logger;

    public MicrosoftTeamsProvider(HttpClient http, IOptions<MicrosoftTeamsOptions> opt, ILogger<MicrosoftTeamsProvider> logger)
    {
        _http = http;
        _opt = opt.Value;
        _logger = logger;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(_opt.ClientId)
            .WithClientSecret(_opt.ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{_opt.TenantId}")
            .Build();
        var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync(ct);
        return result.AccessToken;
    }

    public async Task<Result<CreateMeetingResult>> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await GetAccessTokenAsync(ct);
            var payload = new
            {
                startDateTime = request.StartAt.UtcDateTime.ToString("o"),
                endDateTime = request.EndAt.UtcDateTime.ToString("o"),
                subject = request.Title,
                isOnlineMeeting = true,
                onlineMeetingProvider = "teamsForBusiness"
            };

            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/users/{_opt.OrganizerUserId}/events");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = JsonContent.Create(payload);

            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogError("Teams meeting create failed: {Status} {Body}", resp.StatusCode, err);
                return Error.Provider("teams.failed", "Failed to create Teams meeting.");
            }
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            var meetId = json.GetProperty("id").GetString()!;
            var joinUrl = json.GetProperty("onlineMeeting").GetProperty("joinUrl").GetString()!;
            return new CreateMeetingResult(meetId, joinUrl, DateTimeOffset.UtcNow.AddHours(24));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teams meeting create exception");
            return Error.Provider("teams.unavailable", "Microsoft Teams is unavailable.");
        }
    }

    public Task<bool> DeleteMeetingAsync(string providerMeetingId, CancellationToken ct = default) => Task.FromResult(true);
    public Task<string?> GetRecordingUrlAsync(string providerMeetingId, CancellationToken ct = default) => Task.FromResult<string?>(null);
}
