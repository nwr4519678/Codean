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
            new PagedList<AuditLogResponse>(items, total, request.PageNumber, request.PageSize));
    }
}

public sealed class GetPlatformOverviewHandler
    : IRequestHandler<GetPlatformOverviewQuery, Result<PlatformOverviewResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;
    private readonly IRepository<Course> _courses;
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly IRepository<Payment> _payments;
    private readonly IClock _clock;

    public GetPlatformOverviewHandler(
        IRepository<User> users,
        IRepository<Role> roles,
        IRepository<Course> courses,
        IRepository<CodingSubmission> submissions,
        IRepository<Payment> payments,
        IClock clock)
    {
        _users       = users;
        _roles       = roles;
        _courses     = courses;
        _submissions = submissions;
        _payments    = payments;
        _clock       = clock;
    }

    public async Task<Result<PlatformOverviewResponse>> Handle(
        GetPlatformOverviewQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow.UtcDateTime;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearStart  = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Resolve role IDs dynamically from the Roles table
        var rolesQ = _roles.Query();
        var studentRoleId = rolesQ.Where(r => r.Name == "Student").Select(r => r.Id).FirstOrDefault();
        var teacherRoleId = rolesQ.Where(r => r.Name == "Teacher").Select(r => r.Id).FirstOrDefault();

        var totalStudents    = await _users.CountAsync(u => u.RoleId == studentRoleId, ct);
        var totalTeachers    = await _users.CountAsync(u => u.RoleId == teacherRoleId, ct);
        var totalUsers       = await _users.CountAsync(u => u.Id > 0, ct);
        var activeCourses    = await _courses.CountAsync(c => c.IsPublished, ct);
        var totalSubmissions = await _submissions.CountAsync(s => s.Id > 0, ct);

        var newStudentsThisMonth = await _users.CountAsync(
            u => u.RoleId == studentRoleId && u.CreatedAt >= monthStart, ct);
        var newTeachersThisMonth = await _users.CountAsync(
            u => u.RoleId == teacherRoleId && u.CreatedAt >= monthStart, ct);

        var passedSubmissions = await _submissions.CountAsync(
            s => s.Status == "Accepted", ct);

        var paidPayments = await _payments.ListAsync(p => p.Status == "Paid", ct);
        var totalRevenue = paidPayments.Sum(p => p.Amount);

        // Monthly student registrations for the current year (grouped by month)
        var yearlyStudents = _users.Query()
            .Where(u => u.RoleId == studentRoleId && u.CreatedAt >= yearStart)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToList();

        var monthlyEnrollments = yearlyStudents
            .Select(g => new MonthlyEnrollmentPoint(g.Year, g.Month, g.Count))
            .ToList();

        var overview = new PlatformOverviewResponse(
            totalUsers,
            totalStudents,
            totalTeachers,
            activeCourses,
            totalSubmissions,
            totalRevenue,
            newStudentsThisMonth,
            newTeachersThisMonth,
            passedSubmissions,
            monthlyEnrollments,
            now
        );

        return Result<PlatformOverviewResponse>.Success(overview);
    }
}
