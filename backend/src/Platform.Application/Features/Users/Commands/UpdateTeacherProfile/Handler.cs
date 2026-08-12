using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Users.Dtos;
using Platform.Application.Features.Users.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.UpdateTeacherProfile;

public sealed class UpdateTeacherProfileHandler : IRequestHandler<UpdateTeacherProfileCommand, Result<TeacherProfileResponse>>
{
    private readonly IRepository<TeacherProfile> _teacherProfiles;
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;

    public UpdateTeacherProfileHandler(
        IRepository<TeacherProfile> teacherProfiles,
        IRepository<User> users,
        IUnitOfWork uow,
        ICurrentUser current)
    {
        _teacherProfiles = teacherProfiles;
        _users           = users;
        _uow             = uow;
        _current         = current;
    }

    public async Task<Result<TeacherProfileResponse>> Handle(UpdateTeacherProfileCommand cmd, CancellationToken ct)
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

        var profile = await _teacherProfiles.GetByIdAsync(userId, ct);

        if (profile is null)
        {
            profile = new TeacherProfile
            {
                UserId = userId,
                Biography = cmd.Biography,
                Photo = cmd.Photo,
                Facebook = cmd.Facebook,
                YouTube = cmd.YouTube,
                Website = cmd.Website,
                Experience = cmd.Experience,
                Specialization = cmd.Specialization,
                IsVerified = true
            };
            await _teacherProfiles.AddAsync(profile, ct);
        }
        else
        {
            profile.Biography = cmd.Biography;
            if (!string.IsNullOrWhiteSpace(cmd.Photo)) profile.Photo = cmd.Photo;
            profile.Facebook = cmd.Facebook;
            profile.YouTube = cmd.YouTube;
            profile.Website = cmd.Website;
            profile.Experience = cmd.Experience;
            profile.Specialization = cmd.Specialization;
            _teacherProfiles.Update(profile);
        }

        await _uow.SaveChangesAsync(ct);
        profile.User = user!;

        return profile.ToTeacherProfileResponse();
    }
}
