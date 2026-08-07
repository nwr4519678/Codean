using FluentValidation.TestHelper;
using Platform.Application.Features.Learning.Dtos;
using Platform.Application.Features.Learning.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Learning;

public class LearningValidatorTests
{
    // ── CreateCourseCommandValidator ──────────────────────────────────────────

    public class CreateCourseCommandValidatorTests
    {
        private readonly CreateCourseCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_TitleEmpty()
        {
            var cmd = new CreateCourseCommand("", "Desc", null, "Category", 100);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_HaveError_When_PriceNegative()
        {
            var cmd = new CreateCourseCommand("Title", "Desc", null, "Category", -10);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new CreateCourseCommand("Advanced C#", "Desc", "thumb.jpg", "Programming", 199.99m);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── CreateCourseModuleCommandValidator ────────────────────────────────────

    public class CreateCourseModuleCommandValidatorTests
    {
        private readonly CreateCourseModuleCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_MonthNumberZero()
        {
            var cmd = new CreateCourseModuleCommand(1L, "Module Title", 0, 1, "Desc");
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.MonthNumber);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new CreateCourseModuleCommand(1L, "Month 1 Basics", 1, 1, "Desc");
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }

    // ── TrackLessonProgressCommandValidator ───────────────────────────────────

    public class TrackLessonProgressCommandValidatorTests
    {
        private readonly TrackLessonProgressCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_CompletionExceeds100()
        {
            var cmd = new TrackLessonProgressCommand(1L, 105, 60);
            _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Completion);
        }

        [Fact]
        public void Should_NotHaveError_When_Valid()
        {
            var cmd = new TrackLessonProgressCommand(1L, 85.5m, 180);
            _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
        }
    }
}
