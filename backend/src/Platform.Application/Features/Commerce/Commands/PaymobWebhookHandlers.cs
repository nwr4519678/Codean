using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Payments;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Commerce.Commands;

/// <summary>
/// Idempotent webhook handler for Paymob payment notifications.
///
/// Flow:
///   1. Verify HMAC signature via IPaymobClient.
///   2. Look up Payment row by TransactionId.
///   3. Check idempotency: if already Status == "Paid", return success (no double-processing).
///   4. On payment success:
///        - Update Payment status to "Paid", set PaidAt.
///        - Create active StudentSubscription row.
///        - Generate Invoice record with unique invoice number.
/// </summary>
public sealed class ProcessPaymobWebhookHandler
    : IRequestHandler<ProcessPaymobWebhookCommand, Result<bool>>
{
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<StudentSubscription> _subscriptions;
    private readonly IRepository<SubscriptionPlan> _plans;
    private readonly IRepository<Invoice> _invoices;
    private readonly IUnitOfWork _uow;
    private readonly IPaymobClient _paymob;
    private readonly IClock _clock;

    public ProcessPaymobWebhookHandler(
        IRepository<Payment> payments,
        IRepository<StudentSubscription> subscriptions,
        IRepository<SubscriptionPlan> plans,
        IRepository<Invoice> invoices,
        IUnitOfWork uow,
        IPaymobClient paymob,
        IClock clock)
    {
        _payments      = payments;
        _subscriptions = subscriptions;
        _plans        = plans;
        _invoices       = invoices;
        _uow           = uow;
        _paymob        = paymob;
        _clock         = clock;
    }

    public async Task<Result<bool>> Handle(
        ProcessPaymobWebhookCommand request, CancellationToken ct)
    {
        // 1. Mandatory HMAC Validation
        bool isValidSignature = _paymob.ValidateHmac(request.RawBody, request.Signature);
        if (!isValidSignature)
            return Result<bool>.Failure(
                Error.Unauthorized("paymob.invalid_signature", "Webhook HMAC signature is invalid."));

        // 2. Defence-in-depth verification via Paymob API
        bool isVerified = await _paymob.VerifyTransactionAsync(request.TransactionId, ct);
        if (!isVerified && request.Success)
            return Result<bool>.Failure(
                Error.Validation("paymob.verification_failed", "Transaction verification failed with provider."));

        // 3. Find payment record
        var payment = await _payments.FirstOrDefaultAsync(
            p => p.TransactionId == request.TransactionId ||
                 (!string.IsNullOrWhiteSpace(request.OrderId) && p.TransactionId == request.OrderId), ct);

        if (payment is null)
            return Result<bool>.Failure(
                Error.NotFound("payments.not_found", $"Payment with TransactionId '{request.TransactionId}' not found."));

        if (!string.IsNullOrWhiteSpace(request.OrderId) && payment.TransactionId != request.OrderId)
            return Result<bool>.Failure(Error.Validation("paymob.order_mismatch", "Webhook order does not match the pending payment."));

        if (request.Success && (payment.Amount != request.Amount ||
            !string.Equals(payment.Currency, request.Currency, StringComparison.OrdinalIgnoreCase)))
            return Result<bool>.Failure(Error.Validation("paymob.amount_mismatch", "Webhook amount or currency does not match the pending payment."));

        // 4. Idempotency Check — if already paid, suppress duplicate processing safely
        if (payment.Status == "Paid")
            return Result<bool>.Success(true);

        var now = _clock.UtcNow.UtcDateTime;
        await using var transaction = await _uow.BeginTransactionAsync(ct);

        if (request.Success)
        {
            payment.Status        = "Paid";
            payment.PaidAt        = now;
            payment.PaymentMethod = request.PaymentMethod ?? "Paymob";

            // The pending payment is created from a plan encoded in the merchant order.
            // Resolve it from the order format and require the plan to exist before granting access.
            var planId = TryExtractPlanId(payment.TransactionId);
            var subscriptionPlan = planId.HasValue
                ? await _plans.GetByIdAsync(planId.Value, ct)
                : null;
            if (payment.SubscriptionId is null && subscriptionPlan is null)
                return Result<bool>.Failure(Error.Validation("paymob.plan_not_found", "The subscription plan for this payment no longer exists."));

            if (payment.SubscriptionId is null)
            {
                var start = DateOnly.FromDateTime(now);
                var end = start.AddMonths(subscriptionPlan!.DurationMonths);
                var subscription = new StudentSubscription
                {
                    StudentId = payment.StudentId,
                    TeacherId = subscriptionPlan.TeacherId,
                    PlanId = subscriptionPlan.Id,
                    MonthNumber = subscriptionPlan.MonthNumber,
                    StartDate = start,
                    EndDate = end,
                    AccessExpiresAt = end,
                    Status = "Active",
                    CreatedAt = now
                };
                await _subscriptions.AddAsync(subscription, ct);
                payment.Subscription = subscription;
            }

            // Generate Invoice once, even if a provider retries the callback.
            var existingInvoice = await _invoices.FirstOrDefaultAsync(i => i.PaymentId == payment.Id, ct);
            if (existingInvoice is null)
            {
            var invoiceNumber = $"INV-{now:yyyyMMdd}-{payment.Id:D6}";
            var invoice = new Invoice
            {
                PaymentId     = payment.Id,
                InvoiceNumber = invoiceNumber,
                IssuedAt      = now,
                TotalAmount   = payment.Amount,
                TaxAmount     = Math.Round(payment.Amount * 0.14m, 2), // 14% VAT standard
                PdfUrl        = $"/invoices/{invoiceNumber}.pdf"
            };

            await _invoices.AddAsync(invoice, ct);
            }
            _payments.Update(payment);
        }
        else
        {
            payment.Status = "Failed";
            _payments.Update(payment);
        }

        await _uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result<bool>.Success(true);
    }

    private static long? TryExtractPlanId(string transactionId)
    {
        var parts = transactionId.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3 && long.TryParse(parts[2], out var planId) ? planId : null;
    }
}
