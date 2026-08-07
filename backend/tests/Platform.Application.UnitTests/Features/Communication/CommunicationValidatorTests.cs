using FluentValidation.TestHelper;
using Platform.Application.Features.Communication.Dtos;
using Platform.Application.Features.Communication.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Communication;

public class CommunicationValidatorTests
{
    public class CreateAnnouncementCommandValidatorTests
    {
        private readonly CreateAnnouncementCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_TitleEmpty()
            => _validator.TestValidate(new CreateAnnouncementCommand(1L, "", "Body", false))
                .ShouldHaveValidationErrorFor(x => x.Title);

        [Fact]
        public void Should_HaveError_When_BodyEmpty()
            => _validator.TestValidate(new CreateAnnouncementCommand(1L, "Title", "", false))
                .ShouldHaveValidationErrorFor(x => x.Body);

        [Fact]
        public void Should_NotHaveError_When_Valid()
            => _validator.TestValidate(new CreateAnnouncementCommand(1L, "Title", "Body", true))
                .ShouldNotHaveAnyValidationErrors();
    }

    public class SendNotificationCommandValidatorTests
    {
        private readonly SendNotificationCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_UserIdInvalid()
            => _validator.TestValidate(new SendNotificationCommand(0L, "Title", "Body", "System"))
                .ShouldHaveValidationErrorFor(x => x.UserId);

        [Fact]
        public void Should_NotHaveError_When_Valid()
            => _validator.TestValidate(new SendNotificationCommand(50L, "Payment Success", "Your invoice is ready", "Billing"))
                .ShouldNotHaveAnyValidationErrors();
    }
}
