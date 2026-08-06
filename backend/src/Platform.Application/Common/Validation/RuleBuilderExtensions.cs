using FluentValidation;

namespace Platform.Application.Common.Validation;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 8)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(minimumLength).WithMessage($"Password must be at least {minimumLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 digit.");
    }

    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be a valid E.164 formatted international number.");
    }
}
