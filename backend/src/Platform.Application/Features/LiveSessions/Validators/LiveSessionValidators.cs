using FluentValidation;
using Platform.Application.Features.LiveSessions.Dtos;

namespace Platform.Application.Features.LiveSessions.Validators;

public sealed class ScheduleLiveSessionCommandValidator : AbstractValidator<ScheduleLiveSessionCommand>
{
    public ScheduleLiveSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Session title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");

        RuleFor(x => x.OrganizerEmail)
            .NotEmpty().WithMessage("Organizer email is required.")
            .EmailAddress().WithMessage("Organizer email must be a valid email address.");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required.");
    }
}
