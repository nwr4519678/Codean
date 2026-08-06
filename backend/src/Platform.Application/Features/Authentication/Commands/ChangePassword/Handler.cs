using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Caching;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokens;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ICacheService _cache;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public ChangePasswordHandler(
        IRepository<User> users,
        IRepository<Domain.Entities.RefreshToken> refreshTokens,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ICacheService cache,
        ICurrentUser current,
        IClock clock)
    {
        _users         = users;
        _refreshTokens = refreshTokens;
        _auditLogs     = auditLogs;
        _uow           = uow;
        _hasher        = hasher;
        _cache         = cache;
        _current       = current;
        _clock         = clock;
    }

    public async Task<Result> Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        var user = await _users.GetByIdAsync(_current.UserId.Value, ct);
        if (user is null)
            return Error.NotFound("auth.user_not_found", "User not found.");

        if (!_hasher.Verify(cmd.CurrentPassword, user.PasswordHash))
            return Error.Unauthorized("auth.wrong_password", "Current password is incorrect.");

        var now = _clock.UtcNow.UtcDateTime;
        user.PasswordHash = _hasher.Hash(cmd.NewPassword);
        user.UpdatedAt    = now;
        _users.Update(user);

        // Revoke all active refresh tokens — force re-login on every device
        var activeTokens = await _refreshTokens.ListAsync(
            t => t.UserId == user.Id && t.RevokedAt == null, ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt       = now;
            token.RevokedByIp     = _current.IpAddress;
            token.ReplacedByToken = "PASSWORD_CHANGED";
            _refreshTokens.Update(token);
        }

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = user.Id,
            Action     = "User.PasswordChanged",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Invalidate user-scoped caches — profile may reflect stale data, sessions are all revoked
        await _cache.RemoveAsync(AuthCacheKeys.UserProfile(user.Id), ct);
        await _cache.RemoveAsync(AuthCacheKeys.UserSessions(user.Id), ct);

        return Result.Success();
    }
}
