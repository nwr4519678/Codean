using Platform.Application.Features.LiveSessions.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.LiveSessions.Mapping;

public static class LiveSessionMappingExtensions
{
    public static LiveSessionResponse ToResponse(this LiveSession s) =>
        new(
            s.Id,
            s.TeacherId,
            s.CourseId,
            s.ModuleId,
            s.Title,
            s.MeetingId ?? string.Empty,
            s.MeetingLink ?? string.Empty,
            s.Password,
            s.StartTime,
            s.EndTime,
            s.RecordingLink,
            s.Status ?? "Scheduled",
            s.Provider?.Name ?? string.Empty,
            s.CreatedAt
        );

    public static AttendanceResponse ToResponse(this LiveAttendance a) =>
        new(
            a.Id,
            a.LiveSessionId,
            a.StudentId,
            a.Student?.User?.FullName ?? "Student",
            a.JoinTime ?? default,
            a.LeaveTime
        );
}
