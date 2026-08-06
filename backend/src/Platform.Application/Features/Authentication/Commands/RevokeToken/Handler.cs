using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Caching;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.RevokeToken;

public sealed class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokens;
    private readonly IRepository<UserSession> _userSessions;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public RevokeTokenHandler(
        IRepository<Domain.Entities.RefreshToken> refreshTokens,
        IRepository<UserSession> userSessions,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ICacheService cache,
        ICurrentUser current,
        IClock clock)
    {
        _refreshTokens = refreshTokens;
        _userSessions  = userSessions;
        _auditLogs     = auditLogs;
        _uow           = uow;
        _cache         = cache;
        _current       = current;
        _clock         = clock;
    }

    public async Task<Result> Handle(RevokeTokenCommand cmd, CancellationToken ct)
    {
        var now       = _clock.UtcNow.UtcDateTime;
        var tokenHash = HashToken(cmd.Token);

        var stored = await _refreshTokens.FirstOrDefaultAsync(
            t => t.Token == tokenHash && t.RevokedAt == null, ct);

        if (stored is null)
            return Error.NotFound("auth.token_not_found", "Token not found or already revoked.");

        stored.RevokedAt   = now;
        stored.RevokedByIp = _current.IpAddress;
        _refreshTokens.Update(stored);

        // Close associated active session
        var session = await _userSessions.FirstOrDefaultAsync(
            s => s.UserId == stored.UserId && s.IsActive && s.LogoutAt == null, ct);

        if (session is not null)
        {
            session.IsActive  = false;
            session.LogoutAt  = now;
            _userSessions.Update(session);
        }

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = stored.UserId,
            Action     = "User.LoggedOut",
            EntityType = "RefreshToken",
            EntityId   = stored.UserId,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Session ended — invalidate the sessions list cache for this user
        await _cache.RemoveAsync(AuthCacheKeys.UserSessions(stored.UserId), ct);

        return Result.Success();
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
