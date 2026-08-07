using FluentValidation.TestHelper;
using Platform.Application.Features.Assessment.Dtos;
using Platform.Application.Features.Assessment.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Assessment;

public class AssessmentValidatorTests
{
    // ── CreateExamCommandValidator ────────────────────────────────────────────

    public class CreateExamCommandValidatorTests
    {
        private readonly CreateExamCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_TitleEmpty()
        {
            var cmd = new CreateExamCommand(1L, "", "Desc", 60, 100, 50, null, null);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_HaveError_When_DurationZero()
        {
            var cmd = new CreateExamCommand(1L, "Title", "Desc", 0, 100, 50, null, null);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.DurationMinutes);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new CreateExamCommand(1L, "Final Exam", "Desc", 120, 100, 50, null, null);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── GradeHomeworkSubmissionCommandValidator ───────────────────────────────

    public class GradeHomeworkSubmissionCommandValidatorTests
    {
        private readonly GradeHomeworkSubmissionCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_GradeNegative()
        {
            var cmd = new GradeHomeworkSubmissionCommand(1L, -5, "Bad");
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Grade);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new GradeHomeworkSubmissionCommand(1L, 90, "Great work!");
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }
}
