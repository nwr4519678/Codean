using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.LiveSessions;
using Platform.Application.Features.LiveSessions.Dtos;
using Platform.Application.Features.LiveSessions.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.LiveSessions.Commands;

/// <summary>
/// Schedules a live session by delegating to the correct meeting provider
/// (GoogleMeet or MicrosoftTeams) via strategy dispatch on ProviderId.
/// </summary>
public sealed class ScheduleLiveSessionHandler
    : IRequestHandler<ScheduleLiveSessionCommand, Result<LiveSessionResponse>>
{
    private readonly IRepository<LiveSession> _sessions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IGoogleMeetProvider _googleMeet;
    private readonly IMicrosoftTeamsProvider _teamsProvider;

    public ScheduleLiveSessionHandler(
        IRepository<LiveSession> sessions,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        IGoogleMeetProvider googleMeet,
        IMicrosoftTeamsProvider teamsProvider)
    {
        _sessions      = sessions;
        _uow           = uow;
        _currentUser   = currentUser;
        _clock         = clock;
        _googleMeet    = googleMeet;
        _teamsProvider = teamsProvider;
    }

    public async Task<Result<LiveSessionResponse>> Handle(
        ScheduleLiveSessionCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<LiveSessionResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var meetingRequest = new CreateMeetingRequest(
            request.Title,
            new DateTimeOffset(request.StartTime, TimeSpan.Zero),
            new DateTimeOffset(request.EndTime, TimeSpan.Zero),
            request.OrganizerEmail,
            request.TimeZone
        );

        // Strategy dispatch by provider
        Result<CreateMeetingResult> meetingResult = request.Provider switch
        {
            LiveMeetingProviderType.GoogleMeet     => await _googleMeet.CreateMeetingAsync(meetingRequest, ct),
            LiveMeetingProviderType.MicrosoftTeams => await _teamsProvider.CreateMeetingAsync(meetingRequest, ct),
            _ => Result<CreateMeetingResult>.Failure(
                    Error.Validation("livesession.provider_unknown", $"Unknown provider {request.Provider}."))
        };

        if (!meetingResult.IsSuccess)
            return Result<LiveSessionResponse>.Failure(meetingResult.Error);

        var meeting = meetingResult.Value!;

        var session = new LiveSession
        {
            TeacherId   = _currentUser.UserId.Value,
            CourseId    = request.CourseId,
            ModuleId    = request.ModuleId,
            ProviderId  = (int)request.Provider,
            Title       = request.Title.Trim(),
            MeetingId   = meeting.ProviderMeetingId,
            MeetingLink = meeting.OrganizerJoinUrl,
            StartTime   = request.StartTime,
            EndTime     = request.EndTime,
            Status      = "Scheduled",
            CreatedAt   = _clock.UtcNow.UtcDateTime
        };

        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<LiveSessionResponse>.Success(session.ToResponse());
    }
}

public sealed class CancelLiveSessionHandler
    : IRequestHandler<CancelLiveSessionCommand, Result<bool>>
{
    private readonly IRepository<LiveSession> _sessions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IGoogleMeetProvider _googleMeet;
    private readonly IMicrosoftTeamsProvider _teamsProvider;

    public CancelLiveSessionHandler(
        IRepository<LiveSession> sessions,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IGoogleMeetProvider googleMeet,
        IMicrosoftTeamsProvider teamsProvider)
    {
        _sessions      = sessions;
        _uow           = uow;
        _currentUser   = currentUser;
        _googleMeet    = googleMeet;
        _teamsProvider = teamsProvider;
    }

    public async Task<Result<bool>> Handle(
        CancelLiveSessionCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
            return Result<bool>.Failure(
                Error.NotFound("livesession.not_found", $"Live session {request.SessionId} not found."));

        if (session.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(
                Error.Forbidden("auth.forbidden", "You do not have permission."));

        if (session.Status == "Cancelled")
            return Result<bool>.Success(true); // Idempotent

        // Best-effort provider cleanup (non-blocking failure)
        if (!string.IsNullOrEmpty(session.MeetingId))
        {
            _ = session.ProviderId switch
            {
                1 => _googleMeet.DeleteMeetingAsync(session.MeetingId, ct),
                2 => _teamsProvider.DeleteMeetingAsync(session.MeetingId, ct),
                _ => Task.FromResult(false)
            };
        }

        session.Status = "Cancelled";
        _sessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public sealed class GetLiveSessionsByTeacherHandler
    : IRequestHandler<GetLiveSessionsByTeacherQuery, Result<IReadOnlyList<LiveSessionResponse>>>
{
    private readonly IRepository<LiveSession> _sessions;
    private readonly ICurrentUser _currentUser;

    public GetLiveSessionsByTeacherHandler(IRepository<LiveSession> sessions, ICurrentUser currentUser)
    {
        _sessions    = sessions;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<LiveSessionResponse>>> Handle(
        GetLiveSessionsByTeacherQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<IReadOnlyList<LiveSessionResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long teacherId = _currentUser.UserId.Value;

        var q = _sessions.Query().Where(s => s.TeacherId == teacherId);

        if (request.CourseId.HasValue)
            q = q.Where(s => s.CourseId == request.CourseId.Value);

        var list = q.OrderByDescending(s => s.StartTime).ToList();
        return Result<IReadOnlyList<LiveSessionResponse>>.Success(
            list.Select(s => s.ToResponse()).ToList());
    }
}

public sealed class RecordAttendanceHandler
    : IRequestHandler<RecordAttendanceCommand, Result<AttendanceResponse>>
{
    private readonly IRepository<LiveSession> _sessions;
    private readonly IRepository<LiveAttendance> _attendance;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public RecordAttendanceHandler(
        IRepository<LiveSession> sessions,
        IRepository<LiveAttendance> attendance,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _sessions   = sessions;
        _attendance = attendance;
        _uow        = uow;
        _currentUser = currentUser;
        _clock      = clock;
    }

    public async Task<Result<AttendanceResponse>> Handle(
        RecordAttendanceCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<AttendanceResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;
        var now = _clock.UtcNow.UtcDateTime;

        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
            return Result<AttendanceResponse>.Failure(
                Error.NotFound("livesession.not_found", $"Live session {request.SessionId} not found."));

        if (session.Status != "Live" && session.Status != "Scheduled")
            return Result<AttendanceResponse>.Failure(
                Error.Validation("livesession.not_active", "Session is not currently active."));

        // Idempotency: if already checked in, return existing record
        var existing = await _attendance.FirstOrDefaultAsync(
            a => a.LiveSessionId == request.SessionId && a.StudentId == studentId, ct);

        if (existing is not null)
            return Result<AttendanceResponse>.Success(existing.ToResponse());

        var record = new LiveAttendance
        {
            LiveSessionId = request.SessionId,
            StudentId     = studentId,
            JoinTime      = now
        };

        await _attendance.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<AttendanceResponse>.Success(record.ToResponse());
    }
}

public sealed class GetSessionAttendanceHandler
    : IRequestHandler<GetSessionAttendanceQuery, Result<IReadOnlyList<AttendanceResponse>>>
{
    private readonly IRepository<LiveAttendance> _attendance;
    private readonly ICurrentUser _currentUser;

    public GetSessionAttendanceHandler(IRepository<LiveAttendance> attendance, ICurrentUser currentUser)
    {
        _attendance  = attendance;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<AttendanceResponse>>> Handle(
        GetSessionAttendanceQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<IReadOnlyList<AttendanceResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var list = await _attendance.ListAsync(a => a.LiveSessionId == request.SessionId, ct);
        return Result<IReadOnlyList<AttendanceResponse>>.Success(
            list.OrderBy(a => a.JoinTime).Select(a => a.ToResponse()).ToList());
    }
}
