using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Communication.Dtos;
using Platform.Application.Features.Communication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Communication.Commands;

public sealed class SendNotificationHandler
    : IRequestHandler<SendNotificationCommand, Result<NotificationResponse>>
{
    private readonly IRepository<Notification> _notifications;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public SendNotificationHandler(
        IRepository<Notification> notifications,
        IUnitOfWork uow,
        IClock clock)
    {
        _notifications = notifications;
        _uow           = uow;
        _clock         = clock;
    }

    public async Task<Result<NotificationResponse>> Handle(
        SendNotificationCommand request, CancellationToken ct)
    {
        var notification = new Notification
        {
            UserId    = request.UserId,
            Title     = request.Title.Trim(),
            Body      = request.Body.Trim(),
            Type      = request.Type.Trim(),
            IsRead    = false,
            CreatedAt = _clock.UtcNow.UtcDateTime
        };

        await _notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<NotificationResponse>.Success(notification.ToResponse());
    }
}

public sealed class MarkNotificationAsReadHandler
    : IRequestHandler<MarkNotificationAsReadCommand, Result<bool>>
{
    private readonly IRepository<Notification> _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationAsReadHandler(
        IRepository<Notification> notifications,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _uow           = uow;
        _currentUser   = currentUser;
    }

    public async Task<Result<bool>> Handle(
        MarkNotificationAsReadCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var notification = await _notifications.GetByIdAsync(request.NotificationId, ct);
        if (notification is null)
            return Result<bool>.Failure(
                Error.NotFound("notifications.not_found", $"Notification {request.NotificationId} not found."));

        if (notification.UserId != _currentUser.UserId.Value)
            return Result<bool>.Failure(
                Error.Forbidden("auth.forbidden", "You do not own this notification."));

        if (notification.IsRead)
            return Result<bool>.Success(true); // Idempotent

        notification.IsRead = true;
        _notifications.Update(notification);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public sealed class MarkAllNotificationsAsReadHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<bool>>
{
    private readonly IRepository<Notification> _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public MarkAllNotificationsAsReadHandler(
        IRepository<Notification> notifications,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _uow           = uow;
        _currentUser   = currentUser;
    }

    public async Task<Result<bool>> Handle(
        MarkAllNotificationsAsReadCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long userId = _currentUser.UserId.Value;

        var unread = await _notifications.ListAsync(n => n.UserId == userId && !n.IsRead, ct);
        if (unread.Count == 0)
            return Result<bool>.Success(true);

        foreach (var n in unread)
        {
            n.IsRead = true;
            _notifications.Update(n);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}

public sealed class GetMyNotificationsPagedHandler
    : IRequestHandler<GetMyNotificationsPagedQuery, Result<PagedList<NotificationResponse>>>
{
    private readonly IRepository<Notification> _notifications;
    private readonly ICurrentUser _currentUser;

    public GetMyNotificationsPagedHandler(
        IRepository<Notification> notifications,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _currentUser   = currentUser;
    }

    public async Task<Result<PagedList<NotificationResponse>>> Handle(
        GetMyNotificationsPagedQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<PagedList<NotificationResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long userId = _currentUser.UserId.Value;

        var q = _notifications.Query().Where(n => n.UserId == userId);

        if (request.UnreadOnly.HasValue && request.UnreadOnly.Value)
            q = q.Where(n => !n.IsRead);

        q = q.OrderByDescending(n => n.CreatedAt);

        var total = q.Count();
        var items = q.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList()
                     .Select(n => n.ToResponse())
                     .ToList();

        return Result<PagedList<NotificationResponse>>.Success(
            new PagedList<NotificationResponse>(items, total, request.PageNumber, request.PageSize));
    }
}

public sealed class GetUnreadNotificationCountHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<GetUnreadCountResponse>>
{
    private readonly IRepository<Notification> _notifications;
    private readonly ICurrentUser _currentUser;

    public GetUnreadNotificationCountHandler(
        IRepository<Notification> notifications,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _currentUser   = currentUser;
    }

    public async Task<Result<GetUnreadCountResponse>> Handle(
        GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<GetUnreadCountResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long userId = _currentUser.UserId.Value;
        int count = await _notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        return Result<GetUnreadCountResponse>.Success(new GetUnreadCountResponse(count));
    }
}
