using FluentValidation.TestHelper;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Validators;

public class AuthValidatorsTests
{
    // ── RegisterCommandValidator ─────────────────────────────────────────────

    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_FullNameIsEmpty()
        {
            var cmd = new RegisterCommand("", "user@example.com", "Password123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FullName)
                  .WithErrorMessage("Full name is required.");
        }

        [Fact]
        public void Should_HaveError_When_FullNameExceeds200Chars()
        {
            var longName = new string('a', 201);
            var cmd = new RegisterCommand(longName, "user@example.com", "Password123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_HaveError_When_EmailInvalid()
        {
            var cmd = new RegisterCommand("John", "not-an-email", "Password123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("short")]                 // < 8 chars
        [InlineData("lowercaseonly1!")]       // no uppercase
        [InlineData("UPPERCASEONLY1!")]       // no lowercase
        [InlineData("NoDigitsHere!")]          // no digit
        [InlineData("NoSpecialChar123")]       // no special char
        public void Should_HaveError_When_PasswordWeak(string weakPassword)
        {
            var cmd = new RegisterCommand("John", "user@example.com", weakPassword);
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_NotHaveError_When_CommandValid()
        {
            var cmd = new RegisterCommand("John Doe", "user@example.com", "StrongPass123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── LoginCommandValidator ────────────────────────────────────────────────

    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_FieldsEmpty()
        {
            var cmd = new LoginCommand("", "");
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Email);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new LoginCommand("user@example.com", "any_password");
            var result = _validator.TestValidate(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── ChangePasswordCommandValidator ───────────────────────────────────────

    public class ChangePasswordCommandValidatorTests
    {
        private readonly ChangePasswordCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_NewPasswordEqualsCurrent()
        {
            var cmd = new ChangePasswordCommand("Password123!", "Password123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password must be different from the current password.");
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");
            var result = _validator.TestValidate(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── VerifyTwoFactorCommandValidator ──────────────────────────────────────

    public class VerifyTwoFactorCommandValidatorTests
    {
        private readonly VerifyTwoFactorCommandValidator _validator = new();

        [Theory]
        [InlineData("12345")]   // 5 digits
        [InlineData("1234567")] // 7 digits
        [InlineData("12345a")]  // non-digit
        public void Should_HaveError_When_CodeNot6Digits(string code)
        {
            var cmd = new VerifyTwoFactorCommand("secret", code);
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid6Digits()
        {
            var cmd = new VerifyTwoFactorCommand("secret", "123456");
            var result = _validator.TestValidate(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── RevokeSessionCommandValidator ────────────────────────────────────────

    public class RevokeSessionCommandValidatorTests
    {
        private readonly RevokeSessionCommandValidator _validator = new();

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_When_SessionIdNotPositive(long id)
        {
            var cmd = new RevokeSessionCommand(id);
            var result = _validator.TestValidate(cmd);
            result.ShouldHaveValidationErrorFor(x => x.SessionId);
        }

        [Fact]
        public void Should_NotHaveError_When_SessionIdPositive()
        {
            var cmd = new RevokeSessionCommand(1L);
            var result = _validator.TestValidate(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
