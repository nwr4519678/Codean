using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Queries.GetStudentProfile;

public sealed class GetStudentProfileHandler : IRequestHandler<GetStudentProfileQuery, Result<StudentProfileResponse>>
{
    private readonly IRepository<StudentProfile> _studentProfiles;
    private readonly IRepository<User> _users;

    public GetStudentProfileHandler(IRepository<StudentProfile> studentProfiles, IRepository<User> users)
    {
        _studentProfiles = studentProfiles;
        _users           = users;
    }

    public async Task<Result<StudentProfileResponse>> Handle(GetStudentProfileQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Error.NotFound("users.user_not_found", "User not found.");

        var profile = await _studentProfiles.GetByIdAsync(query.UserId, ct)
                      ?? new StudentProfile { UserId = query.UserId };
        profile.User = user;

        return profile.ToStudentProfileResponse();
    }
}
