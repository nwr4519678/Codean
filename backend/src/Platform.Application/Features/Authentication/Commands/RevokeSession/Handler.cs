using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Caching;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.RevokeSession;

public sealed class RevokeSessionHandler : IRequestHandler<RevokeSessionCommand, Result>
{
    private readonly IRepository<UserSession> _sessions;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public RevokeSessionHandler(
        IRepository<UserSession> sessions,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ICacheService cache,
        ICurrentUser current,
        IClock clock)
    {
        _sessions  = sessions;
        _auditLogs = auditLogs;
        _uow       = uow;
        _cache     = cache;
        _current   = current;
        _clock     = clock;
    }

    public async Task<Result> Handle(RevokeSessionCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        var session = await _sessions.GetByIdAsync(cmd.SessionId, ct);

        if (session is null || session.UserId != _current.UserId.Value)
            return Error.NotFound("auth.session_not_found", "Session not found.");

        if (!session.IsActive)
            return Error.Conflict("auth.session_already_revoked", "This session is already inactive.");

        var now = _clock.UtcNow.UtcDateTime;
        session.IsActive = false;
        session.LogoutAt = now;
        _sessions.Update(session);

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = _current.UserId.Value,
            Action     = "User.SessionRevoked",
            EntityType = "UserSession",
            EntityId   = session.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Session revoked — invalidate the sessions list cache for this user
        await _cache.RemoveAsync(AuthCacheKeys.UserSessions(_current.UserId!.Value), ct);

        return Result.Success();
    }
}
