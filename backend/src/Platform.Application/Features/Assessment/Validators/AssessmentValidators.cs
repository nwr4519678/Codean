using FluentValidation;
using Platform.Application.Features.Assessment.Dtos;

namespace Platform.Application.Features.Assessment.Validators;

public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Exam title is required.")
            .MaximumLength(200).WithMessage("Exam title must not exceed 200 characters.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be at least 1 minute.");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0.");
    }
}

public sealed class CreateHomeworkCommandValidator : AbstractValidator<CreateHomeworkCommand>
{
    public CreateHomeworkCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Homework title is required.")
            .MaximumLength(200).WithMessage("Homework title must not exceed 200 characters.");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0.");
    }
}

public sealed class SubmitHomeworkCommandValidator : AbstractValidator<SubmitHomeworkCommand>
{
    public SubmitHomeworkCommandValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0).WithMessage("Valid HomeworkId is required.");

        RuleFor(x => x.SubmissionType)
            .NotEmpty().WithMessage("Submission type is required.");
    }
}

public sealed class GradeHomeworkSubmissionCommandValidator : AbstractValidator<GradeHomeworkSubmissionCommand>
{
    public GradeHomeworkSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId)
            .GreaterThan(0).WithMessage("Valid SubmissionId is required.");

        RuleFor(x => x.Grade)
            .GreaterThanOrEqualTo(0).WithMessage("Grade cannot be negative.");
    }
}
