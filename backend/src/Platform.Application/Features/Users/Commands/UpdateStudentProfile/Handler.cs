using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.UpdateStudentProfile;

public sealed class UpdateStudentProfileHandler : IRequestHandler<UpdateStudentProfileCommand, Result<StudentProfileResponse>>
{
    private readonly IRepository<StudentProfile> _studentProfiles;
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;

    public UpdateStudentProfileHandler(
        IRepository<StudentProfile> studentProfiles,
        IRepository<User> users,
        IUnitOfWork uow,
        ICurrentUser current)
    {
        _studentProfiles = studentProfiles;
        _users           = users;
        _uow             = uow;
        _current         = current;
    }

    public async Task<Result<StudentProfileResponse>> Handle(UpdateStudentProfileCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        var userId = _current.UserId.Value;
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is not null)
        {
            if (!string.IsNullOrWhiteSpace(cmd.FullName)) user.FullName = cmd.FullName.Trim();
            if (cmd.Phone is not null) user.Phone = cmd.Phone.Trim();
            user.UpdatedAt = System.DateTime.UtcNow;
            _users.Update(user);
        }

        var profile = await _studentProfiles.GetByIdAsync(userId, ct);

        if (profile is null)
        {
            profile = new StudentProfile
            {
                UserId = userId,
                Grade = cmd.Grade,
                School = cmd.School,
                ParentPhone = cmd.ParentPhone,
                ParentPhone2 = cmd.ParentPhone2,
                Notes = cmd.Notes
            };
            await _studentProfiles.AddAsync(profile, ct);
        }
        else
        {
            profile.Grade = cmd.Grade;
            profile.School = cmd.School;
            profile.ParentPhone = cmd.ParentPhone;
            profile.ParentPhone2 = cmd.ParentPhone2;
            profile.Notes = cmd.Notes;
            _studentProfiles.Update(profile);
        }

        await _uow.SaveChangesAsync(ct);
        profile.User = user!;

        return profile.ToStudentProfileResponse();
    }
}
