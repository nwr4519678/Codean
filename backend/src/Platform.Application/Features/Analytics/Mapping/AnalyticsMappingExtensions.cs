using Platform.Application.Features.Analytics.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Analytics.Mapping;

public static class AnalyticsMappingExtensions
{
    public static AuditLogResponse ToResponse(this AuditLog log) =>
        new(
            log.Id,
            log.UserId,
            log.User?.FullName ?? "System",
            log.Action ?? string.Empty,
            log.EntityType ?? string.Empty,
            log.EntityId,
            log.OldValues ?? string.Empty,
            log.NewValues ?? string.Empty,
            log.IpAddress ?? string.Empty,
            log.CreatedAt
        );
}
