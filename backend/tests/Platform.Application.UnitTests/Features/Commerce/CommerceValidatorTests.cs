using FluentValidation.TestHelper;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Application.Features.Commerce.Validators;
using Xunit;

namespace Platform.Application.UnitTests.Features.Commerce;

public class CommerceValidatorTests
{
    public class CreateSubscriptionPlanCommandValidatorTests
    {
        private readonly CreateSubscriptionPlanCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_NameEmpty()
            => _validator.TestValidate(new CreateSubscriptionPlanCommand("", 1, 500, 1, "Desc"))
                .ShouldHaveValidationErrorFor(x => x.Name);

        [Fact]
        public void Should_HaveError_When_PriceZeroOrNegative()
            => _validator.TestValidate(new CreateSubscriptionPlanCommand("Gold", 1, 0, 1, "Desc"))
                .ShouldHaveValidationErrorFor(x => x.Price);

        [Fact]
        public void Should_NotHaveError_When_Valid()
            => _validator.TestValidate(new CreateSubscriptionPlanCommand("Monthly Math Pass", 1, 499.99m, 1, "Access all math"))
                .ShouldNotHaveAnyValidationErrors();
    }

    public class ProcessPaymobWebhookCommandValidatorTests
    {
        private readonly ProcessPaymobWebhookCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_When_TransactionIdEmpty()
            => _validator.TestValidate(new ProcessPaymobWebhookCommand("raw", "sig", "", true, "ord", "Card", 100, "EGP"))
                .ShouldHaveValidationErrorFor(x => x.TransactionId);

        [Fact]
        public void Should_HaveError_When_SignatureEmpty()
            => _validator.TestValidate(new ProcessPaymobWebhookCommand("raw", "", "tx_123", true, "ord", "Card", 100, "EGP"))
                .ShouldHaveValidationErrorFor(x => x.Signature);

        [Fact]
        public void Should_NotHaveError_When_Valid()
            => _validator.TestValidate(new ProcessPaymobWebhookCommand("raw", "valid_hmac_sig", "tx_123", true, "ord_1", "Card", 500, "EGP"))
                .ShouldNotHaveAnyValidationErrors();
    }
}
