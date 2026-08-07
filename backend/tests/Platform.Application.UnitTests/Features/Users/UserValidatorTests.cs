using FluentValidation.TestHelper;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Users;

public class UserValidatorTests
{
    // ── UpdateTeacherProfileCommandValidator ─────────────────────────────────

    public class UpdateTeacherProfileCommandValidatorTests
    {
        private readonly UpdateTeacherProfileCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_BiographyExceeds2000Chars()
        {
            var cmd = new UpdateTeacherProfileCommand(new string('a', 2001), null, null, null, null, null);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Biography);
        }

        [Fact]
        public void Should_NotHaveError_When_AllFieldsValid()
        {
            var cmd = new UpdateTeacherProfileCommand("Bio", "fb.com", "yt.com", "site.com", "5yrs", "Math");
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveError_When_AllFieldsNull()
        {
            var cmd = new UpdateTeacherProfileCommand(null, null, null, null, null, null);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── UpdateStudentProfileCommandValidator ─────────────────────────────────

    public class UpdateStudentProfileCommandValidatorTests
    {
        private readonly UpdateStudentProfileCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_GradeExceedsMaxLength()
        {
            var cmd = new UpdateStudentProfileCommand(new string('a', 101), null, null, null, null);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Grade);
        }

        [Fact]
        public void Should_NotHaveError_When_ValidData()
        {
            var cmd = new UpdateStudentProfileCommand("Grade 10", "Cairo School", "01012345678", null, null);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── SetUserStatusCommandValidator ─────────────────────────────────────────

    public class SetUserStatusCommandValidatorTests
    {
        private readonly SetUserStatusCommandValidator _validator = new();

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_When_UserIdNotPositive(long id)
        {
            _validator.TestValidate(new SetUserStatusCommand(id, true))
                      .ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            _validator.TestValidate(new SetUserStatusCommand(1L, false))
                      .ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── AssignUserRoleCommandValidator ────────────────────────────────────────

    public class AssignUserRoleCommandValidatorTests
    {
        private readonly AssignUserRoleCommandValidator _validator = new();

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public void Should_HaveError_When_RoleIdOutOfRange(int roleId)
        {
            _validator.TestValidate(new AssignUserRoleCommand(1L, roleId))
                      .ShouldHaveValidationErrorFor(x => x.RoleId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_NotHaveError_When_RoleIdValid(int roleId)
        {
            _validator.TestValidate(new AssignUserRoleCommand(1L, roleId))
                      .ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── UploadUserAvatarCommandValidator ──────────────────────────────────────

    public class UploadUserAvatarCommandValidatorTests
    {
        private readonly UploadUserAvatarCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_InvalidContentType()
        {
            var cmd = new UploadUserAvatarCommand("avatar.gif", "image/gif", 1024);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.ContentType);
        }

        [Fact]
        public void Should_HaveError_When_FileSizeExceeds5MB()
        {
            var cmd = new UploadUserAvatarCommand("avatar.jpg", "image/jpeg", 6 * 1024 * 1024);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.SizeBytes);
        }

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("image/webp")]
        public void Should_NotHaveError_When_ValidImage(string contentType)
        {
            var cmd = new UploadUserAvatarCommand("avatar.jpg", contentType, 1024 * 1024);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── GetUsersPagedQueryValidator ───────────────────────────────────────────

    public class GetUsersPagedQueryValidatorTests
    {
        private readonly GetUsersPagedQueryValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_PageSizeExceeds100()
        {
            var query = new GetUsersPagedQuery(1, 101);
            _validator.TestValidate(query).ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Should_NotHaveError_When_ValidPagination()
        {
            var query = new GetUsersPagedQuery(1, 20, "search", 1, true);
            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }
    }
}
