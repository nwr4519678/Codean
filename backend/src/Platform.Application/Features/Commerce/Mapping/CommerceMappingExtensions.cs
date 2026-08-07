using Platform.Application.Features.Commerce.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Commerce.Mapping;

public static class CommerceMappingExtensions
{
    public static SubscriptionPlanResponse ToResponse(this SubscriptionPlan plan) =>
        new(
            plan.Id,
            plan.TeacherId,
            plan.Name,
            plan.MonthNumber,
            plan.Price,
            plan.DurationMonths,
            plan.Description ?? string.Empty,
            plan.IsActive,
            plan.CreatedAt
        );

    public static StudentSubscriptionResponse ToResponse(this StudentSubscription sub) =>
        new(
            sub.Id,
            sub.StudentId,
            sub.TeacherId,
            sub.PlanId,
            sub.Plan?.Name ?? string.Empty,
            sub.MonthNumber,
            sub.StartDate,
            sub.EndDate,
            sub.AccessExpiresAt,
            sub.Status ?? "Active",
            sub.CreatedAt
        );

    public static PaymentResponse ToResponse(this Payment payment) =>
        new(
            payment.Id,
            payment.StudentId,
            payment.SubscriptionId,
            payment.TransactionId ?? string.Empty,
            payment.Amount,
            payment.Currency ?? "EGP",
            payment.Status ?? "Pending",
            payment.PaymentMethod ?? "Card",
            payment.PaidAt,
            payment.CreatedAt,
            payment.Invoice?.ToResponse()
        );

    public static InvoiceResponse ToResponse(this Invoice invoice) =>
        new(
            invoice.Id,
            invoice.PaymentId,
            invoice.InvoiceNumber ?? string.Empty,
            invoice.IssuedAt,
            invoice.TotalAmount,
            invoice.TaxAmount,
            invoice.PdfUrl ?? string.Empty
        );
}
