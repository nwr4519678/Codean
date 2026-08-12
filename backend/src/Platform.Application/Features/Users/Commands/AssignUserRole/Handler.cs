using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.AssignUserRole;

public sealed class AssignUserRoleHandler : IRequestHandler<AssignUserRoleCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;
    private readonly IRepository<TeacherProfile> _teacherProfiles;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public AssignUserRoleHandler(
        IRepository<User> users,
        IRepository<Role> roles,
        IRepository<TeacherProfile> teacherProfiles,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ICurrentUser current,
        IClock clock)
    {
        _users           = users;
        _roles           = roles;
        _teacherProfiles = teacherProfiles;
        _auditLogs       = auditLogs;
        _uow             = uow;
        _current         = current;
        _clock           = clock;
    }

    public async Task<Result> Handle(AssignUserRoleCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        if (user.RoleId == cmd.RoleId)
            return Result.Success();

        var role = await _roles.FirstOrDefaultAsync(r => r.Id == cmd.RoleId, ct);
        if (role is null)
            return Error.NotFound("users.role_not_found", "Role not found.");

        var now = _clock.UtcNow.UtcDateTime;
        var oldRole = user.RoleId;

        user.RoleId = role.Id;
        user.UpdatedAt = now;
        _users.Update(user);

        // Auto-provision TeacherProfile if assigned Teacher role and profile is missing
        if (string.Equals(role.Name, "Teacher", System.StringComparison.OrdinalIgnoreCase))
        {
            var profile = await _teacherProfiles.FirstOrDefaultAsync(t => t.UserId == user.Id, ct);
            if (profile is null)
            {
                await _teacherProfiles.AddAsync(new TeacherProfile
                {
                    UserId     = user.Id,
                    Biography  = "Instructor on Codean Platform",
                    IsVerified = true
                }, ct);
            }
        }

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = _current.UserId,
            Action     = "User.RoleAssigned",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now,
            NewValues  = $"{{\"OldRoleId\":{oldRole},\"NewRoleId\":{cmd.RoleId}}}"
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
