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
    private readonly IRepository<Invoice> _invoices;
    private readonly IUnitOfWork _uow;
    private readonly IPaymobClient _paymob;
    private readonly IClock _clock;

    public ProcessPaymobWebhookHandler(
        IRepository<Payment> payments,
        IRepository<StudentSubscription> subscriptions,
        IRepository<Invoice> invoices,
        IUnitOfWork uow,
        IPaymobClient paymob,
        IClock clock)
    {
        _payments      = payments;
        _subscriptions = subscriptions;
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
            p => p.TransactionId == request.TransactionId, ct);

        if (payment is null)
            return Result<bool>.Failure(
                Error.NotFound("payments.not_found", $"Payment with TransactionId '{request.TransactionId}' not found."));

        // 4. Idempotency Check — if already paid, suppress duplicate processing safely
        if (payment.Status == "Paid")
            return Result<bool>.Success(true);

        var now = _clock.UtcNow.UtcDateTime;

        if (request.Success)
        {
            payment.Status        = "Paid";
            payment.PaidAt        = now;
            payment.PaymentMethod = request.PaymentMethod ?? "Paymob";

            // Generate Invoice
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
            _payments.Update(payment);
        }
        else
        {
            payment.Status = "Failed";
            _payments.Update(payment);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
