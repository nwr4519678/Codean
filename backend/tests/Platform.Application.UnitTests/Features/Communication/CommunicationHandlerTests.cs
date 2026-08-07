using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Communication.Commands;
using Platform.Application.Features.Communication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Communication;

public class CommunicationHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── CreateAnnouncement ────────────────────────────────────────────────────

    public class CreateAnnouncementHandlerTests
    {
        private readonly IRepository<Announcement> _announcements = Substitute.For<IRepository<Announcement>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ICacheService _cache = Substitute.For<ICacheService>();
        private readonly CreateAnnouncementHandler _sut;

        public CreateAnnouncementHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new CreateAnnouncementHandler(_announcements, _uow, _current, _clock, _cache);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(new CreateAnnouncementCommand(1L, "Welcome", "Hello students", true), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCreatePinnedAnnouncement()
        {
            var cmd = new CreateAnnouncementCommand(1L, "Exam Schedule", "Exam is next Monday", true);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Title.Should().Be("Exam Schedule");
            result.Value.IsPinned.Should().BeTrue();

            await _announcements.Received(1).AddAsync(
                Arg.Is<Announcement>(a => a.TeacherId == 10L && a.Title == "Exam Schedule" && a.IsPinned),
                Arg.Any<CancellationToken>());
        }
    }

    // ── MarkNotificationAsRead ────────────────────────────────────────────────

    public class MarkNotificationAsReadHandlerTests
    {
        private readonly IRepository<Notification> _notifications = Substitute.For<IRepository<Notification>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly MarkNotificationAsReadHandler _sut;

        public MarkNotificationAsReadHandlerTests()
        {
            _current.UserId.Returns(50L);
            _sut = new MarkNotificationAsReadHandler(_notifications, _uow, _current);
        }

        [Fact]
        public async Task Handle_WhenNotFound_ShouldReturnNotFound()
        {
            _notifications.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((Notification?)null);

            var result = await _sut.Handle(new MarkNotificationAsReadCommand(99L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("notifications.not_found");
        }

        [Fact]
        public async Task Handle_WhenNotOwner_ShouldReturnForbidden()
        {
            _notifications.GetByIdAsync(1L, Arg.Any<CancellationToken>())
                .Returns(new Notification { Id = 1L, UserId = 999L });

            var result = await _sut.Handle(new MarkNotificationAsReadCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.forbidden");
        }

        [Fact]
        public async Task Handle_WhenOwner_ShouldMarkRead()
        {
            var notif = new Notification { Id = 1L, UserId = 50L, IsRead = false };
            _notifications.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(notif);

            var result = await _sut.Handle(new MarkNotificationAsReadCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            notif.IsRead.Should().BeTrue();
            _notifications.Received(1).Update(notif);
        }
    }

    // ── GetUnreadNotificationCount ────────────────────────────────────────────

    public class GetUnreadNotificationCountHandlerTests
    {
        private readonly IRepository<Notification> _notifications = Substitute.For<IRepository<Notification>>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly GetUnreadNotificationCountHandler _sut;

        public GetUnreadNotificationCountHandlerTests()
        {
            _current.UserId.Returns(50L);
            _sut = new GetUnreadNotificationCountHandler(_notifications, _current);
        }

        [Fact]
        public async Task Handle_ShouldReturnCountOfUnreadNotifications()
        {
            _notifications.CountAsync(Arg.Any<Expression<Func<Notification, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(3);

            var result = await _sut.Handle(new GetUnreadNotificationCountQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.UnreadCount.Should().Be(3);
        }
    }
}
