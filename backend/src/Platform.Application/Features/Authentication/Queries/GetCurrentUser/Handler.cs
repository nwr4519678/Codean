using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;
    private readonly ICurrentUser _current;

    public GetCurrentUserHandler(IRepository<User> users, IRepository<Role> roles, ICurrentUser current)
    {
        _users   = users;
        _roles   = roles;
        _current = current;
    }

    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Error.NotFound("auth.user_not_found", "User not found.");

        var role = await _roles.FirstOrDefaultAsync(r => r.Id == user.RoleId, ct);
        return user.ToCurrentUserResponse(role?.Name ?? "Student");
    }
}
