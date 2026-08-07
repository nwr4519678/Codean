using FluentValidation;
using Platform.Application.Features.Communication.Dtos;

namespace Platform.Application.Features.Communication.Validators;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Announcement title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Announcement body is required.")
            .MaximumLength(4000).WithMessage("Body must not exceed 4000 characters.");
    }
}

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid UserId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Notification title is required.")
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Notification body is required.")
            .MaximumLength(1000).WithMessage("Body must not exceed 1000 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Notification type is required.");
    }
}
