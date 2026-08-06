using Platform.Domain.Results;

namespace Platform.Application.Common.Contracts.LiveSessions;

public sealed record CreateMeetingRequest(
    string Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string OrganizerEmail,
    string TimeZone);

public sealed record CreateMeetingResult(
    string ProviderMeetingId,
    string OrganizerJoinUrl,
    DateTimeOffset? ExpiresAt);

public interface IGoogleMeetProvider
{
    Task<Result<CreateMeetingResult>> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(string providerMeetingId, CancellationToken ct = default);
    Task<string?> GetRecordingUrlAsync(string providerMeetingId, CancellationToken ct = default);
}

public interface IMicrosoftTeamsProvider
{
    Task<Result<CreateMeetingResult>> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(string providerMeetingId, CancellationToken ct = default);
    Task<string?> GetRecordingUrlAsync(string providerMeetingId, CancellationToken ct = default);
}
