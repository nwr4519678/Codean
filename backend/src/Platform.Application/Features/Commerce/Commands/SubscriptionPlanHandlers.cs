using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Caching;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Application.Features.Commerce.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Commerce.Commands;

public sealed class CreateSubscriptionPlanHandler
    : IRequestHandler<CreateSubscriptionPlanCommand, Result<SubscriptionPlanResponse>>
{
    private readonly IRepository<SubscriptionPlan> _plans;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public CreateSubscriptionPlanHandler(
        IRepository<SubscriptionPlan> plans,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _plans = plans;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<SubscriptionPlanResponse>> Handle(
        CreateSubscriptionPlanCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<SubscriptionPlanResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var plan = new SubscriptionPlan
        {
            TeacherId      = _currentUser.UserId.Value,
            Name           = request.Name.Trim(),
            MonthNumber    = request.MonthNumber,
            Price          = request.Price,
            DurationMonths = request.DurationMonths,
            Description    = request.Description?.Trim(),
            IsActive       = true,
            CreatedAt      = _clock.UtcNow.UtcDateTime
        };

        await _plans.AddAsync(plan, ct);
        await _uow.SaveChangesAsync(ct);

        // Invalidate all plan list pages — a new plan appears in listings
        await _cache.RemoveByTagAsync(CacheTags.SubscriptionPlanList, ct);

        return Result<SubscriptionPlanResponse>.Success(plan.ToResponse());
    }
}

public sealed class UpdateSubscriptionPlanHandler
    : IRequestHandler<UpdateSubscriptionPlanCommand, Result<SubscriptionPlanResponse>>
{
    private readonly IRepository<SubscriptionPlan> _plans;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public UpdateSubscriptionPlanHandler(
        IRepository<SubscriptionPlan> plans,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _plans = plans;
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<SubscriptionPlanResponse>> Handle(
        UpdateSubscriptionPlanCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<SubscriptionPlanResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var plan = await _plans.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result<SubscriptionPlanResponse>.Failure(
                Error.NotFound("plans.not_found", $"Plan {request.PlanId} not found."));

        if (plan.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<SubscriptionPlanResponse>.Failure(
                Error.Forbidden("auth.forbidden", "You do not have permission."));

        plan.Name        = request.Name.Trim();
        plan.Price       = request.Price;
        plan.Description = request.Description?.Trim();
        plan.IsActive    = request.IsActive;

        _plans.Update(plan);
        await _uow.SaveChangesAsync(ct);

        // Invalidate both the specific plan detail and all list pages
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.SubscriptionPlanDetail(request.PlanId), ct),
            _cache.RemoveByTagAsync(CacheTags.SubscriptionPlanList, ct));

        return Result<SubscriptionPlanResponse>.Success(plan.ToResponse());
    }
}

public sealed class GetSubscriptionPlanByIdHandler
    : IRequestHandler<GetSubscriptionPlanByIdQuery, Result<SubscriptionPlanResponse>>
{
    private readonly IRepository<SubscriptionPlan> _plans;
    public GetSubscriptionPlanByIdHandler(IRepository<SubscriptionPlan> plans) => _plans = plans;

    public async Task<Result<SubscriptionPlanResponse>> Handle(
        GetSubscriptionPlanByIdQuery request, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result<SubscriptionPlanResponse>.Failure(
                Error.NotFound("plans.not_found", $"Plan {request.PlanId} not found."));

        return Result<SubscriptionPlanResponse>.Success(plan.ToResponse());
    }
}

public sealed class GetSubscriptionPlansHandler
    : IRequestHandler<GetSubscriptionPlansQuery, Result<IReadOnlyList<SubscriptionPlanResponse>>>
{
    private readonly IRepository<SubscriptionPlan> _plans;
    public GetSubscriptionPlansHandler(IRepository<SubscriptionPlan> plans) => _plans = plans;

    public async Task<Result<IReadOnlyList<SubscriptionPlanResponse>>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken ct)
    {
        var q = _plans.Query();

        if (request.TeacherId.HasValue)
            q = q.Where(p => p.TeacherId == request.TeacherId.Value);

        if (request.IsActive.HasValue)
            q = q.Where(p => p.IsActive == request.IsActive.Value);

        var list = q.OrderByDescending(p => p.CreatedAt).ToList();
        var dtos = list.Select(p => p.ToResponse()).ToList();

        return Result<IReadOnlyList<SubscriptionPlanResponse>>.Success(dtos);
    }
}
