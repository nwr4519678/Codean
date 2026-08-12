using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _uow;

    public DeleteUserHandler(IRepository<User> users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        _users.Remove(user);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}