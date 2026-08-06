using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.LiveSessions;
using Platform.Domain.Results;

namespace Platform.Infrastructure.LiveSessions.GoogleMeet;

public sealed class GoogleMeetOptions
{
    public string ServiceAccountJson { get; set; } = "";
    public string ImpersonateUser { get; set; } = "";
    public string ApplicationName { get; set; } = "Platform";
}

public sealed class GoogleMeetProvider : IGoogleMeetProvider
{
    private readonly GoogleMeetOptions _opt;
    private readonly ILogger<GoogleMeetProvider> _logger;
    private readonly Lazy<Task<CalendarService>> _calendar;

    public GoogleMeetProvider(IOptions<GoogleMeetOptions> opt, ILogger<GoogleMeetProvider> logger)
    {
        _opt = opt.Value;
        _logger = logger;
        _calendar = new Lazy<Task<CalendarService>>(BuildAsync);
    }

    private async Task<CalendarService> BuildAsync()
    {
        var credential = GoogleCredential.FromJson(_opt.ServiceAccountJson)
            .CreateScoped(CalendarService.Scope.Calendar)
            .CreateWithUser(_opt.ImpersonateUser);
        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _opt.ApplicationName
        });
    }

    public async Task<Result<CreateMeetingResult>> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        try
        {
            var service = await _calendar.Value;
            var ev = new Event
            {
                Summary = request.Title,
                Start = new EventDateTime { DateTimeDateTimeOffset = request.StartAt, TimeZone = request.TimeZone },
                End = new EventDateTime { DateTimeDateTimeOffset = request.EndAt, TimeZone = request.TimeZone },
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString("N"),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                },
                Attendees = new[] { new EventAttendee { Email = request.OrganizerEmail } }
            };

            var created = await service.Events.Insert(ev, "primary")
                .ExecuteAsync(ct);

            var meetId = created.ConferenceData?.ConferenceId ?? created.Id ?? Guid.NewGuid().ToString();
            var joinUrl = created.HangoutLink ?? created.ConferenceData?.EntryPoints?.FirstOrDefault()?.Uri ?? "";
            return new CreateMeetingResult(meetId, joinUrl, DateTimeOffset.UtcNow.AddHours(24));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Meet create failed");
            return Platform.Domain.Results.Error.Provider("google_meet.failed", "Failed to create Google Meet meeting.");
        }
    }

    public Task<bool> DeleteMeetingAsync(string providerMeetingId, CancellationToken ct = default) =>
        Task.FromResult(true); // Google Meet is implicit on calendar event; the event is deleted elsewhere

    public Task<string?> GetRecordingUrlAsync(string providerMeetingId, CancellationToken ct = default) =>
        Task.FromResult<string?>(null);
}
