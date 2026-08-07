using FluentValidation;
using Platform.Application.Features.Users.Dtos;

namespace Platform.Application.Features.Users.Validators;

public sealed class UpdateTeacherProfileCommandValidator : AbstractValidator<UpdateTeacherProfileCommand>
{
    public UpdateTeacherProfileCommandValidator()
    {
        RuleFor(x => x.Biography)
            .MaximumLength(2000).WithMessage("Biography must not exceed 2000 characters.");

        RuleFor(x => x.Facebook)
            .MaximumLength(500).WithMessage("Facebook link must not exceed 500 characters.");

        RuleFor(x => x.YouTube)
            .MaximumLength(500).WithMessage("YouTube link must not exceed 500 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website link must not exceed 500 characters.");

        RuleFor(x => x.Experience)
            .MaximumLength(1000).WithMessage("Experience must not exceed 1000 characters.");

        RuleFor(x => x.Specialization)
            .MaximumLength(200).WithMessage("Specialization must not exceed 200 characters.");
    }
}

public sealed class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    public UpdateStudentProfileCommandValidator()
    {
        RuleFor(x => x.Grade)
            .MaximumLength(100).WithMessage("Grade must not exceed 100 characters.");

        RuleFor(x => x.School)
            .MaximumLength(200).WithMessage("School name must not exceed 200 characters.");

        RuleFor(x => x.ParentPhone)
            .MaximumLength(50).WithMessage("Parent phone must not exceed 50 characters.");

        RuleFor(x => x.ParentPhone2)
            .MaximumLength(50).WithMessage("Secondary parent phone must not exceed 50 characters.");
    }
}

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required.");
    }
}

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required.");

        RuleFor(x => x.RoleId)
            .InclusiveBetween(1, 3).WithMessage("RoleId must be 1 (Student), 2 (Teacher), or 3 (Admin).");
    }
}

public sealed class UploadUserAvatarCommandValidator : AbstractValidator<UploadUserAvatarCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public UploadUserAvatarCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage("Avatar must be a JPEG, PNG, or WebP image.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than 0.")
            .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("Avatar file size must not exceed 5 MB.");
    }
}

public sealed class GetUsersPagedQueryValidator : AbstractValidator<GetUsersPagedQuery>
{
    public GetUsersPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
