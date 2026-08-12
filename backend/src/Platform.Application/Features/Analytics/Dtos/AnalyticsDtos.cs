using System;
using System.Collections.Generic;
using MediatR;
using Platform.Application.Common.Pagination;
using Platform.Domain.Results;

namespace Platform.Application.Features.Analytics.Dtos;

// ── Audit Log DTOs ────────────────────────────────────────────────────────

public sealed record AuditLogResponse(
    long Id,
    long? UserId,
    string UserName,
    string Action,
    string EntityType,
    long? EntityId,
    string OldValues,
    string NewValues,
    string IpAddress,
    DateTime CreatedAt
);

public sealed record GetAuditLogsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    long? UserId = null,
    string? Action = null,
    string? EntityType = null
) : IRequest<Result<PagedList<AuditLogResponse>>>;

// ── Monthly Enrollment chart data ─────────────────────────────────────────

public sealed record MonthlyEnrollmentPoint(int Year, int Month, int Count);

// ── Platform Overview Analytics DTOs ──────────────────────────────────────

public sealed record PlatformOverviewResponse(
    int TotalUsers,
    int TotalStudents,
    int TotalTeachers,
    int ActiveCourses,
    int TotalSubmissions,
    decimal TotalRevenue,
    int NewStudentsThisMonth,
    int NewTeachersThisMonth,
    int PassedSubmissions,
    IReadOnlyList<MonthlyEnrollmentPoint> MonthlyEnrollments,
    DateTime GeneratedAt
);

public sealed record GetPlatformOverviewQuery()
    : IRequest<Result<PlatformOverviewResponse>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.PlatformOverview;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.PlatformOverview];
}
