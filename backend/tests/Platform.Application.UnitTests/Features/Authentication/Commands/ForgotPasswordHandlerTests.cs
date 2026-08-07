using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Features.Authentication.Commands.ForgotPassword;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class ForgotPasswordHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ForgotPasswordHandler _sut;

    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public ForgotPasswordHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _current.IpAddress.Returns("127.0.0.1");

        _sut = new ForgotPasswordHandler(
            _users,
            _auditLogs,
            _uow,
            _email,
            _current,
            _clock);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var cmd = new ForgotPasswordCommand("nonexistent@example.com");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Anti-enumeration
        _email.DidNotReceiveWithAnyArgs().SendAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_WhenUserInactive_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        var user = new User { Id = 1, Email = "inactive@example.com", IsActive = false };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        var cmd = new ForgotPasswordCommand("inactive@example.com");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Anti-enumeration
        _email.DidNotReceiveWithAnyArgs().SendAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndIsActive_ShouldAddAuditLogSaveAndSendEmail()
    {
        // Arrange
        var user = new User { Id = 10, Email = "active@example.com", IsActive = true };
        _users.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(user);

        var cmd = new ForgotPasswordCommand("active@example.com");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _auditLogs.Received(1).AddAsync(Arg.Is<AuditLog>(a =>
            a.UserId == 10 &&
            a.Action == "User.PasswordResetRequested" &&
            a.NewValues != null &&
            a.NewValues.Contains("TokenHash")
        ), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _email.Received(1).SendAsync(Arg.Is<EmailMessage>(m =>
            m.To == "active@example.com" &&
            m.Subject.Contains("Password Reset")
        ), Arg.Any<CancellationToken>());
    }
}
