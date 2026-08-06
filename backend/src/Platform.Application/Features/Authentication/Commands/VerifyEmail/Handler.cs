using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Caching;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.VerifyEmail;

public sealed class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public VerifyEmailHandler(
        IRepository<User> users,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ICacheService cache,
        ICurrentUser current,
        IClock clock)
    {
        _users     = users;
        _auditLogs = auditLogs;
        _uow       = uow;
        _cache     = cache;
        _current   = current;
        _clock     = clock;
    }

    public async Task<Result> Handle(VerifyEmailCommand cmd, CancellationToken ct)
    {
        var normalized = cmd.Email.Trim().ToLowerInvariant();
        var user       = await _users.FirstOrDefaultAsync(u => u.Email == normalized, ct);

        if (user is null)
            return Error.Failure("auth.verify_invalid", "Invalid verification token.");

        if (user.EmailConfirmed)
            return Error.Conflict("auth.already_verified", "Email address is already verified.");

        var now = _clock.UtcNow.UtcDateTime;
        user.EmailConfirmed = true;
        user.UpdatedAt      = now;
        _users.Update(user);

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = user.Id,
            Action     = "User.EmailVerified",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // EmailConfirmed changed — invalidate the cached profile
        await _cache.RemoveAsync(AuthCacheKeys.UserProfile(user.Id), ct);

        return Result.Success();
    }
}
