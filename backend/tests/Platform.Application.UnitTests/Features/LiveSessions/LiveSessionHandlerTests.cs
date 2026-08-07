using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.LiveSessions;
using Platform.Application.Features.LiveSessions.Commands;
using Platform.Application.Features.LiveSessions.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Application.UnitTests.Features.LiveSessions;

public class LiveSessionHandlerTests
{
    // ── ScheduleLiveSession ───────────────────────────────────────────────────

    public class ScheduleLiveSessionHandlerTests
    {
        private readonly IRepository<LiveSession> _sessions = Substitute.For<IRepository<LiveSession>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly IGoogleMeetProvider _googleMeet = Substitute.For<IGoogleMeetProvider>();
        private readonly IMicrosoftTeamsProvider _teams = Substitute.For<IMicrosoftTeamsProvider>();
        private readonly ScheduleLiveSessionHandler _sut;

        public ScheduleLiveSessionHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new ScheduleLiveSessionHandler(_sessions, _uow, _current, _clock, _googleMeet, _teams);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var cmd = ValidCommand(LiveMeetingProviderType.GoogleMeet);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenGoogleMeetProviderFails_ShouldReturnFailure()
        {
            _googleMeet.CreateMeetingAsync(Arg.Any<CreateMeetingRequest>(), Arg.Any<CancellationToken>())
                .Returns(Result<CreateMeetingResult>.Failure(
                    Error.Validation("provider.error", "Google Meet API unreachable.")));

            var result = await _sut.Handle(ValidCommand(LiveMeetingProviderType.GoogleMeet), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("provider.error");
        }

        [Fact]
        public async Task Handle_WhenGoogleMeet_ShouldCreateSessionWithMeetingDetails()
        {
            _googleMeet.CreateMeetingAsync(Arg.Any<CreateMeetingRequest>(), Arg.Any<CancellationToken>())
                .Returns(Result<CreateMeetingResult>.Success(new CreateMeetingResult(
                    "gmeet_abc123", "https://meet.google.com/abc-defg-hij", null)));

            var result = await _sut.Handle(ValidCommand(LiveMeetingProviderType.GoogleMeet), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.MeetingId.Should().Be("gmeet_abc123");
            result.Value.MeetingLink.Should().Be("https://meet.google.com/abc-defg-hij");
            result.Value.Status.Should().Be("Scheduled");

            await _sessions.Received(1).AddAsync(
                Arg.Is<LiveSession>(s => s.TeacherId == 10L && s.ProviderId == 1),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenMicrosoftTeams_ShouldDelegateToTeamsProvider()
        {
            _teams.CreateMeetingAsync(Arg.Any<CreateMeetingRequest>(), Arg.Any<CancellationToken>())
                .Returns(Result<CreateMeetingResult>.Success(new CreateMeetingResult(
                    "teams_xyz789", "https://teams.microsoft.com/l/meetup/xyz", null)));

            var result = await _sut.Handle(ValidCommand(LiveMeetingProviderType.MicrosoftTeams), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.MeetingId.Should().Be("teams_xyz789");
            result.Value.ProviderName.Should().Be(string.Empty); // no nav prop loaded in test

            await _googleMeet.DidNotReceive().CreateMeetingAsync(Arg.Any<CreateMeetingRequest>(), Arg.Any<CancellationToken>());
        }

        private static ScheduleLiveSessionCommand ValidCommand(LiveMeetingProviderType provider) =>
            new(1L, null, "Math Live Session",
                DateTime.UtcNow.AddHours(2),
                DateTime.UtcNow.AddHours(3),
                provider,
                "teacher@school.com",
                "UTC");
    }

    // ── RecordAttendance (Idempotent) ─────────────────────────────────────────

    public class RecordAttendanceHandlerTests
    {
        private readonly IRepository<LiveSession> _sessions = Substitute.For<IRepository<LiveSession>>();
        private readonly IRepository<LiveAttendance> _attendance = Substitute.For<IRepository<LiveAttendance>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly RecordAttendanceHandler _sut;

        public RecordAttendanceHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(50L);
            _sut = new RecordAttendanceHandler(_sessions, _attendance, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenSessionNotFound_ShouldReturnNotFound()
        {
            _sessions.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((LiveSession?)null);

            var result = await _sut.Handle(new RecordAttendanceCommand(99L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("livesession.not_found");
        }

        [Fact]
        public async Task Handle_WhenAlreadyAttended_ShouldBeIdempotentWithoutInsert()
        {
            _sessions.GetByIdAsync(1L, Arg.Any<CancellationToken>())
                .Returns(new LiveSession { Id = 1L, Status = "Live" });

            var existing = new LiveAttendance { Id = 5L, LiveSessionId = 1L, StudentId = 50L };
            _attendance.FirstOrDefaultAsync(Arg.Any<Expression<Func<LiveAttendance, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(existing);

            var result = await _sut.Handle(new RecordAttendanceCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _attendance.DidNotReceive().AddAsync(Arg.Any<LiveAttendance>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenFirstTime_ShouldCreateAttendanceRecord()
        {
            _sessions.GetByIdAsync(1L, Arg.Any<CancellationToken>())
                .Returns(new LiveSession { Id = 1L, Status = "Live" });

            _attendance.FirstOrDefaultAsync(Arg.Any<Expression<Func<LiveAttendance, bool>>>(), Arg.Any<CancellationToken>())
                .Returns((LiveAttendance?)null);

            var result = await _sut.Handle(new RecordAttendanceCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _attendance.Received(1).AddAsync(
                Arg.Is<LiveAttendance>(a => a.LiveSessionId == 1L && a.StudentId == 50L),
                Arg.Any<CancellationToken>());
        }
    }
}
