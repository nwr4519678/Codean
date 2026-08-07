using FluentValidation.TestHelper;
using Platform.Application.Features.Judge.Dtos;
using Platform.Application.Features.Judge.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Judge;

public class JudgeValidatorTests
{
    public class CreateCodingChallengeValidatorTests
    {
        private readonly CreateCodingChallengeCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_TitleEmpty()
            => _validator.TestValidate(
                    new CreateCodingChallengeCommand("", "Desc", "python", "pass", "[]", "Easy", 10))
                .ShouldHaveValidationErrorFor(x => x.Title);

        [Theory]
        [InlineData("brainfuck")]
        [InlineData("cobol")]
        public void Should_HaveError_When_LanguageUnsupported(string lang)
            => _validator.TestValidate(
                    new CreateCodingChallengeCommand("T", "D", lang, "pass", "[]", "Easy", 10))
                .ShouldHaveValidationErrorFor(x => x.Language);

        [Theory]
        [InlineData("Beginner")]
        [InlineData("Expert")]
        public void Should_HaveError_When_DifficultyInvalid(string diff)
            => _validator.TestValidate(
                    new CreateCodingChallengeCommand("T", "D", "python", "pass", "[]", diff, 10))
                .ShouldHaveValidationErrorFor(x => x.Difficulty);

        [Theory]
        [InlineData("python")]
        [InlineData("csharp")]
        [InlineData("rust")]
        public void Should_NotHaveError_When_LanguageValid(string lang)
            => _validator.TestValidate(
                    new CreateCodingChallengeCommand("Title", "Desc", lang, "pass", "[{}]", "Hard", 100))
                .ShouldNotHaveAnyValidationErrors();
    }

    public class SubmitCodeChallengeValidatorTests
    {
        private readonly SubmitCodeChallengeCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_SourceCodeEmpty()
            => _validator.TestValidate(
                    new SubmitCodeChallengeCommand(1L, "", "python"))
                .ShouldHaveValidationErrorFor(x => x.SourceCode);

        [Fact]
        public void Should_HaveError_When_SourceCodeTooLong()
            => _validator.TestValidate(
                    new SubmitCodeChallengeCommand(1L, new string('x', 50_001), "python"))
                .ShouldHaveValidationErrorFor(x => x.SourceCode);

        [Fact]
        public void Should_NotHaveError_When_Valid()
            => _validator.TestValidate(
                    new SubmitCodeChallengeCommand(1L, "print('hello')", "python3"))
                .ShouldNotHaveAnyValidationErrors();
    }
}
