using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Payments;
using Platform.Application.Features.Commerce.Commands;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Application.UnitTests.Features.Commerce;

public class CommerceHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── CreateSubscriptionPlan ────────────────────────────────────────────────

    public class CreateSubscriptionPlanHandlerTests
    {
        private readonly IRepository<SubscriptionPlan> _plans = Substitute.For<IRepository<SubscriptionPlan>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ICacheService _cache = Substitute.For<ICacheService>();
        private readonly CreateSubscriptionPlanHandler _sut;

        public CreateSubscriptionPlanHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new CreateSubscriptionPlanHandler(_plans, _uow, _current, _clock, _cache);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(
                new CreateSubscriptionPlanCommand("Monthly Pass", 1, 500, 1, "Access all math lessons"),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCreateActivePlan()
        {
            var cmd = new CreateSubscriptionPlanCommand("Monthly Math Pass", 1, 500, 1, "Desc");

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Name.Should().Be("Monthly Math Pass");
            result.Value.Price.Should().Be(500);
            result.Value.IsActive.Should().BeTrue();

            await _plans.Received(1).AddAsync(
                Arg.Is<SubscriptionPlan>(p => p.TeacherId == 10L && p.Price == 500),
                Arg.Any<CancellationToken>());
        }
    }

    // ── InitiateCheckout ──────────────────────────────────────────────────────

    public class InitiateCheckoutHandlerTests
    {
        private readonly IRepository<SubscriptionPlan> _plans = Substitute.For<IRepository<SubscriptionPlan>>();
        private readonly IRepository<Payment> _payments = Substitute.For<IRepository<Payment>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IPaymobClient _paymob = Substitute.For<IPaymobClient>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly InitiateCheckoutHandler _sut;

        public InitiateCheckoutHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(50L);
            _sut = new InitiateCheckoutHandler(_plans, _payments, _uow, _paymob, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenPlanNotFound_ShouldReturnNotFound()
        {
            _plans.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((SubscriptionPlan?)null);

            var result = await _sut.Handle(new InitiateCheckoutCommand(99L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("plans.not_found");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCallPaymobAndStorePendingPayment()
        {
            _plans.GetByIdAsync(1L, Arg.Any<CancellationToken>())
                .Returns(new SubscriptionPlan { Id = 1, Name = "Gold Plan", Price = 300, IsActive = true });

            _paymob.CreateIntentionAsync(
                Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(),
                Arg.Any<IEnumerable<(string, int, decimal)>>(), Arg.Any<long>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Result<PaymobIntention>.Success(new PaymobIntention(
                    "secret_123", "https://accept.paymob.com/checkout", "ord_999", 300, "EGP")));

            var result = await _sut.Handle(new InitiateCheckoutCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.CheckoutUrl.Should().Be("https://accept.paymob.com/checkout");
            result.Value.Amount.Should().Be(300);

            await _payments.Received(1).AddAsync(
                Arg.Is<Payment>(p => p.StudentId == 50L && p.Amount == 300 && p.Status == "Pending"),
                Arg.Any<CancellationToken>());
        }
    }

    // ── ProcessPaymobWebhook ──────────────────────────────────────────────────

    public class ProcessPaymobWebhookHandlerTests
    {
        private readonly IRepository<Payment> _payments = Substitute.For<IRepository<Payment>>();
        private readonly IRepository<StudentSubscription> _subs = Substitute.For<IRepository<StudentSubscription>>();
        private readonly IRepository<SubscriptionPlan> _plans = Substitute.For<IRepository<SubscriptionPlan>>();
        private readonly IRepository<Invoice> _invoices = Substitute.For<IRepository<Invoice>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IPaymobClient _paymob = Substitute.For<IPaymobClient>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ProcessPaymobWebhookHandler _sut;

        public ProcessPaymobWebhookHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _sut = new ProcessPaymobWebhookHandler(_payments, _subs, _plans, _invoices, _uow, _paymob, _clock);
        }

        [Fact]
        public async Task Handle_WhenHMACInvalid_ShouldReturnUnauthorized()
        {
            _paymob.ValidateHmac(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var cmd = new ProcessPaymobWebhookCommand("raw", "bad_sig", "tx_1", true, "ord_1", "Card", 300, "EGP");
            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("paymob.invalid_signature");
        }

        [Fact]
        public async Task Handle_WhenAlreadyPaid_ShouldBeIdempotentWithoutRecreatingInvoice()
        {
            _paymob.ValidateHmac(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _paymob.VerifyTransactionAsync("tx_1", Arg.Any<CancellationToken>()).Returns(true);

            var existingPayment = new Payment { Id = 10L, TransactionId = "tx_1", Status = "Paid", Currency = "EGP", Amount = 300 };
            _payments.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(existingPayment);

            var cmd = new ProcessPaymobWebhookCommand("raw", "valid_sig", "tx_1", true, "tx_1", "Card", 300, "EGP");
            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _invoices.DidNotReceive().AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenFirstTimeSuccess_ShouldUpdatePaymentAndCreateInvoice()
        {
            _paymob.ValidateHmac(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _paymob.VerifyTransactionAsync("sub_plan_1_50_tx_2", Arg.Any<CancellationToken>()).Returns(true);

            var pendingPayment = new Payment { Id = 15L, TransactionId = "sub_plan_1_50_tx_2", Amount = 300, Currency = "EGP", Status = "Pending" };
            _payments.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(pendingPayment);

            _plans.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(new SubscriptionPlan { Id = 1, TeacherId = 7, DurationMonths = 1, Price = 300, IsActive = true });
            var cmd = new ProcessPaymobWebhookCommand("raw", "sig", "sub_plan_1_50_tx_2", true, "sub_plan_1_50_tx_2", "Card", 300, "EGP");
            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            pendingPayment.Status.Should().Be("Paid");

            await _invoices.Received(1).AddAsync(
                Arg.Is<Invoice>(i => i.PaymentId == 15L && i.TotalAmount == 300),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
