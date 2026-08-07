using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IRepository<User> _users;

    public GetUserByIdHandler(IRepository<User> users)
    {
        _users = users;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        return user.ToUserResponse();
    }
}
