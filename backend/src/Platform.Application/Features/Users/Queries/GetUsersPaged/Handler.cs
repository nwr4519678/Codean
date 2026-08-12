using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Queries.GetUsersPaged;

public sealed class GetUsersPagedHandler : IRequestHandler<GetUsersPagedQuery, Result<PagedList<UserResponse>>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;

    public GetUsersPagedHandler(IRepository<User> users, IRepository<Role> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<Result<PagedList<UserResponse>>> Handle(GetUsersPagedQuery query, CancellationToken ct)
    {
        var usersQ = _users.Query();
        var rolesQ = _roles.Query();

        if (query.RoleId.HasValue)
            usersQ = usersQ.Where(u => u.RoleId == query.RoleId.Value);

        if (query.IsActive.HasValue)
            usersQ = usersQ.Where(u => u.IsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            usersQ = usersQ.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        // Join with Roles table so role name is resolved from actual DB data, not hardcoded IDs
        var projected = from u in usersQ.OrderByDescending(u => u.CreatedAt)
                        join r in rolesQ on u.RoleId equals r.Id into rg
                        from role in rg.DefaultIfEmpty()
                        select new UserResponse(
                            u.Id,
                            u.Email,
                            u.FullName,
                            u.Phone,
                            role != null ? role.Name : "Student",
                            u.IsActive,
                            u.EmailConfirmed,
                            u.LastLogin,
                            u.CreatedAt);

        var paged = await PagedList<UserResponse>.CreateAsync(projected, query.PageNumber, query.PageSize, ct);
        return paged;
    }
}
