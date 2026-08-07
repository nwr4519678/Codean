using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Queries.GetUsersPaged;

public sealed class GetUsersPagedHandler : IRequestHandler<GetUsersPagedQuery, Result<PagedList<UserResponse>>>
{
    private readonly IRepository<User> _users;

    public GetUsersPagedHandler(IRepository<User> users)
    {
        _users = users;
    }

    public async Task<Result<PagedList<UserResponse>>> Handle(GetUsersPagedQuery query, CancellationToken ct)
    {
        var q = _users.Query();

        if (query.RoleId.HasValue)
            q = q.Where(u => u.RoleId == query.RoleId.Value);

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        q = q.OrderByDescending(u => u.CreatedAt);

        var projected = q.Select(u => u.ToUserResponse());
        var paged = await PagedList<UserResponse>.CreateAsync(projected, query.PageNumber, query.PageSize, ct);

        return paged;
    }
}
