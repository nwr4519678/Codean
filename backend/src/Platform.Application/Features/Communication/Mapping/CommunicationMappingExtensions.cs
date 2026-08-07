using Platform.Application.Features.Communication.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Communication.Mapping;

public static class CommunicationMappingExtensions
{
    public static AnnouncementResponse ToResponse(this Announcement announcement) =>
        new(
            announcement.Id,
            announcement.TeacherId,
            announcement.CourseId,
            announcement.Title,
            announcement.Body ?? string.Empty,
            announcement.IsPinned,
            announcement.PublishedAt
        );

    public static NotificationResponse ToResponse(this Notification notification) =>
        new(
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Body ?? string.Empty,
            notification.Type ?? "System",
            notification.IsRead,
            notification.CreatedAt
        );
}
