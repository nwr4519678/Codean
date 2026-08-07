using System;
using System.Collections.Generic;
using MediatR;
using Platform.Application.Common.Pagination;
using Platform.Domain.Results;

namespace Platform.Application.Features.Communication.Dtos;

// ── Announcement DTOs ─────────────────────────────────────────────────────

public sealed record AnnouncementResponse(
    long Id,
    long TeacherId,
    long? CourseId,
    string Title,
    string Body,
    bool IsPinned,
    DateTime PublishedAt
);

public sealed record CreateAnnouncementCommand(
    long? CourseId,
    string Title,
    string Body,
    bool IsPinned
) : IRequest<Result<AnnouncementResponse>>;

public sealed record UpdateAnnouncementCommand(
    long AnnouncementId,
    string Title,
    string Body,
    bool IsPinned
) : IRequest<Result<AnnouncementResponse>>;

public sealed record DeleteAnnouncementCommand(long AnnouncementId)
    : IRequest<Result<bool>>;

public sealed record GetAnnouncementsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    long? CourseId = null,
    long? TeacherId = null
) : IRequest<Result<PagedList<AnnouncementResponse>>>, Platform.Application.Common.Caching.ICacheableRequest
{
    public string CacheKey => Platform.Application.Common.Caching.CacheKeys.AnnouncementsPaged(PageNumber, PageSize, CourseId, TeacherId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IReadOnlyList<string> Tags => [Platform.Application.Common.Caching.CacheTags.AnnouncementList];
}

// ── Notification DTOs ─────────────────────────────────────────────────────

public sealed record NotificationResponse(
    long Id,
    long UserId,
    string Title,
    string Body,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);

public sealed record SendNotificationCommand(
    long UserId,
    string Title,
    string Body,
    string Type
) : IRequest<Result<NotificationResponse>>;

public sealed record MarkNotificationAsReadCommand(long NotificationId)
    : IRequest<Result<bool>>;

public sealed record MarkAllNotificationsAsReadCommand()
    : IRequest<Result<bool>>;

public sealed record GetMyNotificationsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? UnreadOnly = null
) : IRequest<Result<PagedList<NotificationResponse>>>;

public sealed record GetUnreadCountResponse(int UnreadCount);

public sealed record GetUnreadNotificationCountQuery()
    : IRequest<Result<GetUnreadCountResponse>>;
