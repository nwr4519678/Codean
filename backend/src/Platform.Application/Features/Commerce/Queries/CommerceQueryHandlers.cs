using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Application.Features.Commerce.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Commerce.Queries;

public sealed class GetMySubscriptionsHandler
    : IRequestHandler<GetMySubscriptionsQuery, Result<IReadOnlyList<StudentSubscriptionResponse>>>
{
    private readonly IRepository<StudentSubscription> _subscriptions;
    private readonly ICurrentUser _currentUser;

    public GetMySubscriptionsHandler(
        IRepository<StudentSubscription> subscriptions,
        ICurrentUser currentUser)
    {
        _subscriptions = subscriptions;
        _currentUser   = currentUser;
    }

    public async Task<Result<IReadOnlyList<StudentSubscriptionResponse>>> Handle(
        GetMySubscriptionsQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<IReadOnlyList<StudentSubscriptionResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var list = await _subscriptions.ListAsync(s => s.StudentId == studentId, ct);
        var dtos = list
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.ToResponse())
            .ToList();

        return Result<IReadOnlyList<StudentSubscriptionResponse>>.Success(dtos);
    }
}

public sealed class GetMyPaymentsPagedHandler
    : IRequestHandler<GetMyPaymentsPagedQuery, Result<PagedList<PaymentResponse>>>
{
    private readonly IRepository<Payment> _payments;
    private readonly ICurrentUser _currentUser;

    public GetMyPaymentsPagedHandler(
        IRepository<Payment> payments,
        ICurrentUser currentUser)
    {
        _payments    = payments;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedList<PaymentResponse>>> Handle(
        GetMyPaymentsPagedQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<PagedList<PaymentResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var q = _payments.Query()
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(p => p.ToResponse())
                     .ToList();

        return Result<PagedList<PaymentResponse>>.Success(
            new PagedList<PaymentResponse>(items, request.PageNumber, request.PageSize, total));
    }
}
