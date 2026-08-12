using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Storage;
using Platform.Application.Features.Users.Commands.AssignUserRole;
using Platform.Application.Features.Users.Commands.SetUserStatus;
using Platform.Application.Features.Users.Commands.UpdateStudentProfile;
using Platform.Application.Features.Users.Commands.UpdateTeacherProfile;
using Platform.Application.Features.Users.Commands.UploadUserAvatar;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Queries.GetStudentProfile;
using Platform.Application.Features.Users.Queries.GetTeacherProfile;
using Platform.Application.Features.Users.Queries.GetUserById;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Users;

public class UserHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── UpdateTeacherProfile ──────────────────────────────────────────────────

    public class UpdateTeacherProfileHandlerTests
    {
        private readonly IRepository<TeacherProfile> _teacherProfiles = Substitute.For<IRepository<TeacherProfile>>();
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly UpdateTeacherProfileHandler _sut;

        public UpdateTeacherProfileHandlerTests()
        {
            _current.UserId.Returns(10L);
            _sut = new UpdateTeacherProfileHandler(_teacherProfiles, _users, _uow, _current);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var cmd = new UpdateTeacherProfileCommand("Bio", null, null, null, null, null);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenNoExistingProfile_ShouldCreateNewProfileAndReturnResponse()
        {
            _teacherProfiles.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns((TeacherProfile?)null);
            _users.GetByIdAsync(10L, Arg.Any<CancellationToken>())
                  .Returns(new User { Id = 10, Email = "t@test.com", FullName = "Teacher" });

            var cmd = new UpdateTeacherProfileCommand("My bio", "fb.com/me", null, "mysite.com", "5 years", "Math");

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Biography.Should().Be("My bio");
            result.Value.Specialization.Should().Be("Math");
            await _teacherProfiles.Received(1).AddAsync(Arg.Is<TeacherProfile>(p =>
                p.UserId == 10 &&
                p.Biography == "My bio" &&
                p.Experience == "5 years"
            ), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenExistingProfile_ShouldUpdateAndReturnResponse()
        {
            var profile = new TeacherProfile { UserId = 10, Biography = "Old bio" };
            _teacherProfiles.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns(profile);
            _users.GetByIdAsync(10L, Arg.Any<CancellationToken>())
                  .Returns(new User { Id = 10, Email = "t@test.com", FullName = "Teacher" });

            var cmd = new UpdateTeacherProfileCommand("New bio", null, null, null, null, "Physics");

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            profile.Biography.Should().Be("New bio");
            profile.Specialization.Should().Be("Physics");
            _teacherProfiles.Received(1).Update(profile);
        }
    }

    // ── UpdateStudentProfile ──────────────────────────────────────────────────

    public class UpdateStudentProfileHandlerTests
    {
        private readonly IRepository<StudentProfile> _studentProfiles = Substitute.For<IRepository<StudentProfile>>();
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly UpdateStudentProfileHandler _sut;

        public UpdateStudentProfileHandlerTests()
        {
            _current.UserId.Returns(20L);
            _sut = new UpdateStudentProfileHandler(_studentProfiles, _users, _uow, _current);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(new UpdateStudentProfileCommand(null, null, null, null, null), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValidCommand_ShouldUpsertProfileAndReturnResponse()
        {
            _studentProfiles.GetByIdAsync(20L, Arg.Any<CancellationToken>()).Returns((StudentProfile?)null);
            _users.GetByIdAsync(20L, Arg.Any<CancellationToken>())
                  .Returns(new User { Id = 20, Email = "s@test.com", FullName = "Student" });

            var cmd = new UpdateStudentProfileCommand("Grade 10", "Cairo School", "01012345678", null, "Good student");

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Grade.Should().Be("Grade 10");
            result.Value.School.Should().Be("Cairo School");
        }
    }

    // ── SetUserStatus ─────────────────────────────────────────────────────────

    public class SetUserStatusHandlerTests
    {
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly SetUserStatusHandler _sut;
        private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

        public SetUserStatusHandlerTests()
        {
            _clock.UtcNow.Returns(_now);
            _current.UserId.Returns(1L);
            _sut = new SetUserStatusHandler(_users, _auditLogs, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnNotFound()
        {
            _users.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((User?)null);
            var result = await _sut.Handle(new SetUserStatusCommand(99L, true), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("users.user_not_found");
        }

        [Fact]
        public async Task Handle_WhenStatusAlreadySame_ShouldReturnSuccessWithoutSaving()
        {
            _users.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(new User { Id = 5, IsActive = true });
            var result = await _sut.Handle(new SetUserStatusCommand(5L, true), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenDeactivating_ShouldSetIsActiveFalseAndAuditLog()
        {
            var user = new User { Id = 5, IsActive = true };
            _users.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(user);

            var result = await _sut.Handle(new SetUserStatusCommand(5L, false), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            user.IsActive.Should().BeFalse();
            await _auditLogs.Received(1).AddAsync(Arg.Is<AuditLog>(a =>
                a.Action == "User.Deactivated" && a.EntityId == 5
            ), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ── AssignUserRole ────────────────────────────────────────────────────────

    public class AssignUserRoleHandlerTests
    {
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly IRepository<Role> _roles = Substitute.For<IRepository<Role>>();
        private readonly IRepository<TeacherProfile> _teacherProfiles = Substitute.For<IRepository<TeacherProfile>>();
        private readonly IRepository<AuditLog> _auditLogs = Substitute.For<IRepository<AuditLog>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly AssignUserRoleHandler _sut;
        private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

        public AssignUserRoleHandlerTests()
        {
            _clock.UtcNow.Returns(_now);
            _current.UserId.Returns(1L);
            _sut = new AssignUserRoleHandler(_users, _roles, _teacherProfiles, _auditLogs, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnNotFound()
        {
            _users.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((User?)null);
            var result = await _sut.Handle(new AssignUserRoleCommand(99L, 2), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("users.user_not_found");
        }

        [Fact]
        public async Task Handle_WhenRoleSame_ShouldReturnSuccessWithoutSaving()
        {
            _users.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(new User { Id = 5, RoleId = 2 });
            var result = await _sut.Handle(new AssignUserRoleCommand(5L, 2), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenChangingRole_ShouldUpdateRoleAndAuditLog()
        {
            var user = new User { Id = 5, RoleId = 1 };
            _users.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(user);
            _roles.FirstOrDefaultAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>())
                  .Returns(new Role { Id = 2, Name = "Teacher" });

            var result = await _sut.Handle(new AssignUserRoleCommand(5L, 2), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            user.RoleId.Should().Be(2);
            await _auditLogs.Received(1).AddAsync(Arg.Is<AuditLog>(a =>
                a.Action == "User.RoleAssigned" &&
                a.NewValues!.Contains("\"NewRoleId\":2")
            ), Arg.Any<CancellationToken>());
        }
    }

    // ── UploadUserAvatar ──────────────────────────────────────────────────────

    public class UploadUserAvatarHandlerTests
    {
        private readonly IObjectStorage _storage = Substitute.For<IObjectStorage>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly UploadUserAvatarHandler _sut;

        public UploadUserAvatarHandlerTests()
        {
            _current.UserId.Returns(10L);
            _sut = new UploadUserAvatarHandler(_storage, _current);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(new UploadUserAvatarCommand("photo.jpg", "image/jpeg", 1024), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldReturnPreSignedUploadUrl()
        {
            var expires = DateTimeOffset.UtcNow.AddMinutes(15);
            _storage.GetUploadUrlAsync(Arg.Any<UploadUrlRequest>(), Arg.Any<CancellationToken>())
                    .Returns(Platform.Domain.Results.Result<UploadUrlResult>.Success(
                        new UploadUrlResult("https://r2.example.com/upload", "PUT",
                            new Dictionary<string, string>(), expires)));

            var result = await _sut.Handle(new UploadUserAvatarCommand("avatar.png", "image/png", 2 * 1024 * 1024), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.UploadUrl.Should().Be("https://r2.example.com/upload");
            result.Value.Key.Should().StartWith("avatars/user_10_");
        }
    }

    // ── GetUserById ───────────────────────────────────────────────────────────

    public class GetUserByIdHandlerTests
    {
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly GetUserByIdHandler _sut;

        public GetUserByIdHandlerTests() => _sut = new GetUserByIdHandler(_users);

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnNotFound()
        {
            _users.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((User?)null);
            var result = await _sut.Handle(new GetUserByIdQuery(99L), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("users.user_not_found");
        }

        [Fact]
        public async Task Handle_WhenUserFound_ShouldReturnMappedResponse()
        {
            _users.GetByIdAsync(5L, Arg.Any<CancellationToken>())
                  .Returns(new User { Id = 5, Email = "u@test.com", FullName = "Name", RoleId = 1, IsActive = true });

            var result = await _sut.Handle(new GetUserByIdQuery(5L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Id.Should().Be(5);
            result.Value.Role.Should().Be("Student");
        }
    }
}
