using FluentValidation;
using Platform.Application.Features.Authentication.Dtos;

namespace Platform.Application.Features.Authentication.Validators;

// ═══════════════════════════════════════════════════════════════════════════════
//  Authentication — FluentValidation Validators
//  Registered automatically via AddValidatorsFromAssembly() in DI.
//  Executed by the ValidationBehaviour pipeline before any handler runs.
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least 1 digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character.");
    }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Refresh token is required.");
    }
}

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
    }
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least 1 digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from the current password.");
    }
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least 1 digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character.");
    }
}

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.");
    }
}

public sealed class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.Secret)
            .NotEmpty().WithMessage("TOTP secret is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("TOTP code is required.")
            .Length(6).WithMessage("TOTP code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("TOTP code must contain only digits.");
    }
}

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("A valid session ID is required.");
    }
}
