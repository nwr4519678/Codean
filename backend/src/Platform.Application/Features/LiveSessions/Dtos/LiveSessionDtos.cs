using System;
using System.Collections.Generic;
using MediatR;
using Platform.Domain.Results;

namespace Platform.Application.Features.LiveSessions.Dtos;

// ── Live Session DTOs ─────────────────────────────────────────────────────

public sealed record LiveSessionResponse(
    long Id,
    long TeacherId,
    long? CourseId,
    long? ModuleId,
    string Title,
    string MeetingId,
    string MeetingLink,
    string? Password,
    DateTime StartTime,
    DateTime? EndTime,
    string? RecordingLink,
    string Status,
    string ProviderName,
    DateTime CreatedAt
);

/// <summary>
/// Provider strategies supported. Maps to LiveMeetingProvider.Id in DB.
/// </summary>
public enum LiveMeetingProviderType
{
    GoogleMeet   = 1,
    MicrosoftTeams = 2
}

public sealed record ScheduleLiveSessionCommand(
    long? CourseId,
    long? ModuleId,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    LiveMeetingProviderType Provider,
    string OrganizerEmail,
    string TimeZone
) : IRequest<Result<LiveSessionResponse>>;

public sealed record CancelLiveSessionCommand(long SessionId)
    : IRequest<Result<bool>>;

public sealed record GetLiveSessionsByTeacherQuery(
    long? CourseId = null
) : IRequest<Result<IReadOnlyList<LiveSessionResponse>>>;

public sealed record GetMyLiveSessionsQuery()
    : IRequest<Result<IReadOnlyList<LiveSessionResponse>>>;

// ── Attendance DTOs ───────────────────────────────────────────────────────

public sealed record AttendanceResponse(
    long Id,
    long SessionId,
    long StudentId,
    string StudentName,
    DateTime JoinedAt,
    DateTime? LeftAt
);

public sealed record RecordAttendanceCommand(
    long SessionId
) : IRequest<Result<AttendanceResponse>>;

public sealed record GetSessionAttendanceQuery(long SessionId)
    : IRequest<Result<IReadOnlyList<AttendanceResponse>>>;
