using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Features.Authentication.Commands.Register;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Commands;

public class RegisterHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly IRepository<Role> _roles = Substitute.For<IRepository<Role>>();
    private readonly IRepository<TeacherProfile> _teacherProfiles = Substitute.For<IRepository<TeacherProfile>>();
    private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RegisterHandler _sut;

    public RegisterHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
        _hasher.Hash(Arg.Any<string>()).Returns("hashed_password");
        _current.IpAddress.Returns("127.0.0.1");

        _sut = new RegisterHandler(
            _users,
            _roles,
            _teacherProfiles,
            _auditLogs,
            _uow,
            _hasher,
            _email,
            _current,
            _clock);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var cmd = new RegisterCommand("John Doe", "existing@example.com", "Password123!");
        _users.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(true);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.email_in_use");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddUserAuditLogAndReturnSuccess()
    {
        // Arrange
        var cmd = new RegisterCommand(" John Doe ", " USER@Domain.com ", "Password123!", " 123456 ");
        _users.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(false);
        _roles.FirstOrDefaultAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>())
              .Returns(new Role { Id = 1, Name = "Student" });

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("user@domain.com");
        result.Value.FullName.Should().Be("John Doe");

        await _users.Received(1).AddAsync(Arg.Is<User>(u =>
            u.FullName == "John Doe" &&
            u.Email == "user@domain.com" &&
            u.Phone == "123456" &&
            u.PasswordHash == "hashed_password" &&
            u.RoleId == 1 &&
            u.IsActive == true &&
            u.EmailConfirmed == false
        ), Arg.Any<CancellationToken>());

        await _auditLogs.Received(1).AddAsync(Arg.Is<AuditLog>(a =>
            a.Action == "User.Registered" &&
            a.EntityType == "User" &&
            a.IpAddress == "127.0.0.1"
        ), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _email.Received(1).SendAsync(Arg.Is<EmailMessage>(m =>
            m.To == "user@domain.com" &&
            m.Subject.Contains("Verify Your Email")
        ), Arg.Any<CancellationToken>());
    }
}
