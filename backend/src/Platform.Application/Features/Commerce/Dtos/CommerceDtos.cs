using System;
using System.Collections.Generic;
using MediatR;
using Platform.Application.Common.Pagination;
using Platform.Domain.Results;

namespace Platform.Application.Features.Commerce.Dtos;

// ── Subscription Plan DTOs ────────────────────────────────────────────────

public sealed record SubscriptionPlanResponse(
    long Id,
    long TeacherId,
    string Name,
    int MonthNumber,
    decimal Price,
    int DurationMonths,
    string Description,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record CreateSubscriptionPlanCommand(
    string Name,
    int MonthNumber,
    decimal Price,
    int DurationMonths,
    string Description
) : IRequest<Result<SubscriptionPlanResponse>>;

public sealed record UpdateSubscriptionPlanCommand(
    long PlanId,
    string Name,
    decimal Price,
    string Description,
    bool IsActive
) : IRequest<Result<SubscriptionPlanResponse>>;

public sealed record GetSubscriptionPlanByIdQuery(long PlanId)
    : IRequest<Result<SubscriptionPlanResponse>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.SubscriptionPlanById(PlanId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.SubscriptionPlanDetail(PlanId), Platform.Application.Common.Caching.CacheTags.SubscriptionPlanList];
}

public sealed record GetSubscriptionPlansQuery(
    long? TeacherId = null,
    bool? IsActive = null
) : IRequest<Result<IReadOnlyList<SubscriptionPlanResponse>>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.SubscriptionPlans(TeacherId, IsActive);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.SubscriptionPlanList];
}

// ── Checkout & Subscription DTOs ──────────────────────────────────────────

public sealed record InitiateCheckoutCommand(
    long PlanId
) : IRequest<Result<CheckoutResponse>>;

public sealed record CheckoutResponse(
    string TransactionId,
    string CheckoutUrl,
    string ClientSecret,
    decimal Amount,
    string Currency
);

public sealed record StudentSubscriptionResponse(
    long Id,
    long StudentId,
    long TeacherId,
    long PlanId,
    string PlanName,
    int MonthNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly AccessExpiresAt,
    string Status,
    DateTime CreatedAt
);

public sealed record GetMySubscriptionsQuery()
    : IRequest<Result<IReadOnlyList<StudentSubscriptionResponse>>>;

// ── Payment & Invoice DTOs ────────────────────────────────────────────────

public sealed record PaymentResponse(
    long Id,
    long StudentId,
    long? SubscriptionId,
    string TransactionId,
    decimal Amount,
    string Currency,
    string Status,
    string PaymentMethod,
    DateTime? PaidAt,
    DateTime CreatedAt,
    InvoiceResponse? Invoice
);

public sealed record InvoiceResponse(
    long Id,
    long PaymentId,
    string InvoiceNumber,
    DateTime IssuedAt,
    decimal TotalAmount,
    decimal TaxAmount,
    string PdfUrl
);

public sealed record GetMyPaymentsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedList<PaymentResponse>>>;

// ── Webhook DTO ───────────────────────────────────────────────────────────

public sealed record ProcessPaymobWebhookCommand(
    string RawBody,
    string Signature,
    string TransactionId,
    bool Success,
    string? OrderId,
    string? PaymentMethod,
    decimal Amount,
    string Currency
) : IRequest<Result<bool>>;
