using FluentValidation;
using Platform.Application.Features.Judge.Dtos;

namespace Platform.Application.Features.Judge.Validators;

public sealed class CreateCodingChallengeCommandValidator : AbstractValidator<CreateCodingChallengeCommand>
{
    private static readonly HashSet<string> SupportedLanguages =
    [
        "python", "python3", "csharp", "java", "cpp", "c", "javascript", "typescript", "go", "rust"
    ];

    public CreateCodingChallengeCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Challenge title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Must(lang => SupportedLanguages.Contains(lang.ToLowerInvariant()))
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}.");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required.")
            .Must(d => d is "Easy" or "Medium" or "Hard")
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");

        RuleFor(x => x.Marks)
            .GreaterThan(0).WithMessage("Marks must be greater than 0.");

        RuleFor(x => x.TestCases)
            .NotEmpty().WithMessage("At least one test case is required.");
    }
}

public sealed class SubmitCodeChallengeCommandValidator : AbstractValidator<SubmitCodeChallengeCommand>
{
    private static readonly HashSet<string> SupportedLanguages =
    [
        "python", "python3", "csharp", "java", "cpp", "c", "javascript", "typescript", "go", "rust"
    ];

    public SubmitCodeChallengeCommandValidator()
    {
        RuleFor(x => x.ChallengeId)
            .GreaterThan(0).WithMessage("Valid ChallengeId is required.");

        RuleFor(x => x.SourceCode)
            .NotEmpty().WithMessage("Source code is required.")
            .MaximumLength(50_000).WithMessage("Source code must not exceed 50,000 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Must(lang => SupportedLanguages.Contains(lang.ToLowerInvariant()))
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}.");
    }
}
