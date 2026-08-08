using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Caching;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Communication.Dtos;
using Platform.Application.Features.Communication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Communication.Commands;

public sealed class CreateAnnouncementHandler
    : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementResponse>>
{
    private readonly IRepository<Announcement> _announcements;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public CreateAnnouncementHandler(
        IRepository<Announcement> announcements,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _announcements = announcements;
        _uow           = uow;
        _currentUser   = currentUser;
        _clock         = clock;
        _cache         = cache;
    }

    public async Task<Result<AnnouncementResponse>> Handle(
        CreateAnnouncementCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<AnnouncementResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var announcement = new Announcement
        {
            TeacherId   = _currentUser.UserId.Value,
            CourseId    = request.CourseId,
            Title       = request.Title.Trim(),
            Body        = request.Body.Trim(),
            IsPinned    = request.IsPinned,
            PublishedAt = _clock.UtcNow.UtcDateTime
        };

        await _announcements.AddAsync(announcement, ct);
        await _uow.SaveChangesAsync(ct);

        // New announcement affects all listing pages
        await _cache.RemoveByTagAsync(CacheTags.AnnouncementList, ct);

        return Result<AnnouncementResponse>.Success(announcement.ToResponse());
    }
}

public sealed class UpdateAnnouncementHandler
    : IRequestHandler<UpdateAnnouncementCommand, Result<AnnouncementResponse>>
{
    private readonly IRepository<Announcement> _announcements;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public UpdateAnnouncementHandler(
        IRepository<Announcement> announcements,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _announcements = announcements;
        _uow           = uow;
        _currentUser   = currentUser;
        _cache         = cache;
    }

    public async Task<Result<AnnouncementResponse>> Handle(
        UpdateAnnouncementCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<AnnouncementResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var announcement = await _announcements.GetByIdAsync(request.AnnouncementId, ct);
        if (announcement is null)
            return Result<AnnouncementResponse>.Failure(
                Error.NotFound("announcements.not_found", $"Announcement {request.AnnouncementId} not found."));

        if (announcement.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<AnnouncementResponse>.Failure(
                Error.Forbidden("auth.forbidden", "You do not have permission."));

        announcement.Title    = request.Title.Trim();
        announcement.Body     = request.Body.Trim();
        announcement.IsPinned = request.IsPinned;

        _announcements.Update(announcement);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.AnnouncementList, ct);

        return Result<AnnouncementResponse>.Success(announcement.ToResponse());
    }
}

public sealed class DeleteAnnouncementHandler
    : IRequestHandler<DeleteAnnouncementCommand, Result<bool>>
{
    private readonly IRepository<Announcement> _announcements;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public DeleteAnnouncementHandler(
        IRepository<Announcement> announcements,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _announcements = announcements;
        _uow           = uow;
        _currentUser   = currentUser;
        _cache         = cache;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnnouncementCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var announcement = await _announcements.GetByIdAsync(request.AnnouncementId, ct);
        if (announcement is null)
            return Result<bool>.Failure(
                Error.NotFound("announcements.not_found", $"Announcement {request.AnnouncementId} not found."));

        if (announcement.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(
                Error.Forbidden("auth.forbidden", "You do not have permission."));

        _announcements.Remove(announcement);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.AnnouncementList, ct);

        return Result<bool>.Success(true);
    }
}

public sealed class GetAnnouncementsPagedHandler
    : IRequestHandler<GetAnnouncementsPagedQuery, Result<PagedList<AnnouncementResponse>>>
{
    private readonly IRepository<Announcement> _announcements;
    public GetAnnouncementsPagedHandler(IRepository<Announcement> announcements)
        => _announcements = announcements;

    public async Task<Result<PagedList<AnnouncementResponse>>> Handle(
        GetAnnouncementsPagedQuery request, CancellationToken ct)
    {
        var q = _announcements.Query();

        if (request.CourseId.HasValue)
            q = q.Where(a => a.CourseId == request.CourseId.Value);

        if (request.TeacherId.HasValue)
            q = q.Where(a => a.TeacherId == request.TeacherId.Value);

        // Pinned first, then newest
        q = q.OrderByDescending(a => a.IsPinned)
             .ThenByDescending(a => a.PublishedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(a => a.ToResponse())
                     .ToList();

        return Result<PagedList<AnnouncementResponse>>.Success(
            new PagedList<AnnouncementResponse>(items, total, request.PageNumber, request.PageSize));
    }
}
