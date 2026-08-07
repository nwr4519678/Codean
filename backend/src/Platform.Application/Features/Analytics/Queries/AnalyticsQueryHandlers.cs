using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Analytics.Dtos;
using Platform.Application.Features.Analytics.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Analytics.Queries;

public sealed class GetAuditLogsPagedHandler
    : IRequestHandler<GetAuditLogsPagedQuery, Result<PagedList<AuditLogResponse>>>
{
    private readonly IRepository<AuditLog> _logs;

    public GetAuditLogsPagedHandler(IRepository<AuditLog> logs)
    {
        _logs = logs;
    }

    public async Task<Result<PagedList<AuditLogResponse>>> Handle(
        GetAuditLogsPagedQuery request, CancellationToken ct)
    {
        var q = _logs.Query();

        if (request.UserId.HasValue)
            q = q.Where(l => l.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            q = q.Where(l => l.Action == request.Action.Trim());

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            q = q.Where(l => l.EntityType == request.EntityType.Trim());

        q = q.OrderByDescending(l => l.CreatedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(l => l.ToResponse())
                     .ToList();

        return Result<PagedList<AuditLogResponse>>.Success(
            new PagedList<AuditLogResponse>(items, request.PageNumber, request.PageSize, total));
    }
}

public sealed class GetPlatformOverviewHandler
    : IRequestHandler<GetPlatformOverviewQuery, Result<PlatformOverviewResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Course> _courses;
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly IRepository<Payment> _payments;
    private readonly IClock _clock;

    public GetPlatformOverviewHandler(
        IRepository<User> users,
        IRepository<Course> courses,
        IRepository<CodingSubmission> submissions,
        IRepository<Payment> payments,
        IClock clock)
    {
        _users       = users;
        _courses     = courses;
        _submissions = submissions;
        _payments    = payments;
        _clock       = clock;
    }

    public async Task<Result<PlatformOverviewResponse>> Handle(
        GetPlatformOverviewQuery request, CancellationToken ct)
    {
        var totalUsers       = await _users.CountAsync(u => u.Id > 0, ct);
        var totalStudents    = await _users.CountAsync(u => u.RoleId == 1, ct);
        var totalTeachers    = await _users.CountAsync(u => u.RoleId == 2, ct);
        var activeCourses    = await _courses.CountAsync(c => c.IsPublished, ct);
        var totalSubmissions = await _submissions.CountAsync(s => s.Id > 0, ct);

        var paidPayments  = await _payments.ListAsync(p => p.Status == "Paid", ct);
        var totalRevenue  = paidPayments.Sum(p => p.Amount);

        var overview = new PlatformOverviewResponse(
            totalUsers,
            totalStudents,
            totalTeachers,
            activeCourses,
            totalSubmissions,
            totalRevenue,
            _clock.UtcNow.UtcDateTime
        );

        return Result<PlatformOverviewResponse>.Success(overview);
    }
}
