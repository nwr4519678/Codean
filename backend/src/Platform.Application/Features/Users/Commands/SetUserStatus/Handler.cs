using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.SetUserStatus;

public sealed class SetUserStatusHandler : IRequestHandler<SetUserStatusCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public SetUserStatusHandler(
        IRepository<User> users,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ICurrentUser current,
        IClock clock)
    {
        _users     = users;
        _auditLogs = auditLogs;
        _uow       = uow;
        _current   = current;
        _clock     = clock;
    }

    public async Task<Result> Handle(SetUserStatusCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        if (user.IsActive == cmd.IsActive)
            return Result.Success();

        var now = _clock.UtcNow.UtcDateTime;
        user.IsActive = cmd.IsActive;
        user.UpdatedAt = now;
        _users.Update(user);

        var action = cmd.IsActive ? "User.Activated" : "User.Deactivated";
        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = _current.UserId,
            Action     = action,
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
