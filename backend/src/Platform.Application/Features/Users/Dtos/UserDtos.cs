using System;
using System.Collections.Generic;
using MediatR;
using Platform.Application.Common.Pagination;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Dtos;

// ── Response DTOs ─────────────────────────────────────────────────────────────

public sealed record UserResponse(
    long Id,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime? LastLogin,
    DateTime CreatedAt);

public sealed record TeacherProfileResponse(
    long UserId,
    string Email,
    string FullName,
    string? Phone,
    string? Biography,
    string? Photo,
    string? Facebook,
    string? YouTube,
    string? Website,
    string? Experience,
    string? Specialization,
    bool IsVerified,
    DateTime? ApprovedAt);

public sealed record StudentProfileResponse(
    long UserId,
    string Email,
    string FullName,
    string? Phone,
    string? Grade,
    string? School,
    string? ParentPhone,
    string? ParentPhone2,
    string? Notes);

public sealed record AvatarUploadResponse(
    string Key,
    string UploadUrl,
    DateTimeOffset ExpiresAt);

// ── Command DTOs ──────────────────────────────────────────────────────────────

public sealed record UpdateTeacherProfileCommand(
    string? Biography = null,
    string? Facebook = null,
    string? YouTube = null,
    string? Website = null,
    string? Experience = null,
    string? Specialization = null,
    string? FullName = null,
    string? Phone = null,
    string? Photo = null)
    : IRequest<Result<TeacherProfileResponse>>;

public sealed record UpdateStudentProfileCommand(
    string? Grade = null,
    string? School = null,
    string? ParentPhone = null,
    string? ParentPhone2 = null,
    string? Notes = null,
    string? FullName = null,
    string? Phone = null)
    : IRequest<Result<StudentProfileResponse>>;

public sealed record SetUserStatusCommand(
    long UserId,
    bool IsActive)
    : IRequest<Result>;

public sealed record AssignUserRoleCommand(
    long UserId,
    int RoleId)
    : IRequest<Result>;

public sealed record DeleteUserCommand(long UserId)
    : IRequest<Result>;

public sealed record UploadUserAvatarCommand(
    string FileName,
    string ContentType,
    long SizeBytes)
    : IRequest<Result<AvatarUploadResponse>>;

// ── Query DTOs ────────────────────────────────────────────────────────────────

public sealed record GetUsersPagedQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    int? RoleId = null,
    bool? IsActive = null)
    : IRequest<Result<PagedList<UserResponse>>>;

public sealed record GetUserByIdQuery(long UserId)
    : IRequest<Result<UserResponse>>;

public sealed record GetTeacherProfileQuery(long UserId)
    : IRequest<Result<TeacherProfileResponse>>;

public sealed record GetStudentProfileQuery(long UserId)
    : IRequest<Result<StudentProfileResponse>>;
