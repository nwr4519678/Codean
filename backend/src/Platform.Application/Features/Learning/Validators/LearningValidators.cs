using FluentValidation;
using Platform.Application.Features.Learning.Dtos;

namespace Platform.Application.Features.Learning.Validators;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MaximumLength(200).WithMessage("Course title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Course description must not exceed 4000 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Course category is required.")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Course price cannot be negative.");
    }
}

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Valid CourseId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MaximumLength(200).WithMessage("Course title must not exceed 200 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Course category is required.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Course price cannot be negative.");
    }
}

public sealed class CreateCourseModuleCommandValidator : AbstractValidator<CreateCourseModuleCommand>
{
    public CreateCourseModuleCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Valid CourseId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Module title is required.")
            .MaximumLength(200).WithMessage("Module title must not exceed 200 characters.");

        RuleFor(x => x.MonthNumber)
            .GreaterThan(0).WithMessage("Month number must be greater than 0.");
    }
}

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("Valid ModuleId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lesson title is required.")
            .MaximumLength(200).WithMessage("Lesson title must not exceed 200 characters.");

        RuleFor(x => x.Duration)
            .GreaterThanOrEqualTo(0).When(x => x.Duration.HasValue)
            .WithMessage("Duration cannot be negative.");
    }
}

public sealed class TrackLessonProgressCommandValidator : AbstractValidator<TrackLessonProgressCommand>
{
    public TrackLessonProgressCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0).WithMessage("Valid LessonId is required.");

        RuleFor(x => x.Completion)
            .InclusiveBetween(0, 100).WithMessage("Completion percentage must be between 0 and 100.");

        RuleFor(x => x.WatchTime)
            .GreaterThanOrEqualTo(0).WithMessage("Watch time cannot be negative.");
    }
}
