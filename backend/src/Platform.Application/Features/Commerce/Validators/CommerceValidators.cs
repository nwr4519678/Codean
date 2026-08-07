using FluentValidation;
using Platform.Application.Features.Commerce.Dtos;

namespace Platform.Application.Features.Commerce.Validators;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(150).WithMessage("Plan name must not exceed 150 characters.");

        RuleFor(x => x.MonthNumber)
            .GreaterThan(0).WithMessage("Month number must be greater than 0.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.DurationMonths)
            .GreaterThan(0).WithMessage("Duration in months must be at least 1.");
    }
}

public sealed class InitiateCheckoutCommandValidator : AbstractValidator<InitiateCheckoutCommand>
{
    public InitiateCheckoutCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Valid PlanId is required.");
    }
}

public sealed class ProcessPaymobWebhookCommandValidator : AbstractValidator<ProcessPaymobWebhookCommand>
{
    public ProcessPaymobWebhookCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Webhook signature is required.");
    }
}
