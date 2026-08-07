using System.Collections.Generic;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Users.Mapping;

public static class UserMappingExtensions
{
    public static UserResponse ToUserResponse(this User user) =>
        new(
            Id:             user.Id,
            Email:          user.Email,
            FullName:       user.FullName,
            Phone:          user.Phone,
            Role:           ResolveRole(user.RoleId),
            IsActive:       user.IsActive,
            EmailConfirmed: user.EmailConfirmed,
            LastLogin:      user.LastLogin,
            CreatedAt:      user.CreatedAt);

    public static TeacherProfileResponse ToTeacherProfileResponse(this TeacherProfile profile) =>
        new(
            UserId:         profile.UserId,
            Email:          profile.User?.Email ?? string.Empty,
            FullName:       profile.User?.FullName ?? string.Empty,
            Biography:      profile.Biography,
            Photo:          profile.Photo,
            Facebook:       profile.Facebook,
            YouTube:        profile.YouTube,
            Website:        profile.Website,
            Experience:     profile.Experience,
            Specialization: profile.Specialization,
            IsVerified:     profile.IsVerified,
            ApprovedAt:     profile.ApprovedAt);

    public static StudentProfileResponse ToStudentProfileResponse(this StudentProfile profile) =>
        new(
            UserId:       profile.UserId,
            Email:        profile.User?.Email ?? string.Empty,
            FullName:     profile.User?.FullName ?? string.Empty,
            Grade:        profile.Grade,
            School:       profile.School,
            ParentPhone:  profile.ParentPhone,
            ParentPhone2: profile.ParentPhone2,
            Notes:        profile.Notes);

    private static string ResolveRole(int roleId) => roleId switch
    {
        2 => "Teacher",
        3 => "Admin",
        _ => "Student"
    };
}
