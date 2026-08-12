using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Queries.GetTeacherProfile;

public sealed class GetTeacherProfileHandler : IRequestHandler<GetTeacherProfileQuery, Result<TeacherProfileResponse>>
{
    private readonly IRepository<TeacherProfile> _teacherProfiles;
    private readonly IRepository<User> _users;

    public GetTeacherProfileHandler(IRepository<TeacherProfile> teacherProfiles, IRepository<User> users)
    {
        _teacherProfiles = teacherProfiles;
        _users           = users;
    }

    public async Task<Result<TeacherProfileResponse>> Handle(GetTeacherProfileQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        var profile = await _teacherProfiles.GetByIdAsync(query.UserId, ct)
                      ?? new TeacherProfile { UserId = query.UserId, IsVerified = true };
        profile.User = user;

        return profile.ToTeacherProfileResponse();
    }
}
