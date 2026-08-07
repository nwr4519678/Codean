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
        var profile = await _studentProfiles.GetByIdAsync(query.UserId, ct);
        if (profile is null)
            return Error.NotFound("users.student_profile_not_found", "Student profile not found.");

        var user = await _users.GetByIdAsync(query.UserId, ct);
        profile.User = user!;

        return profile.ToStudentProfileResponse();
    }
}
