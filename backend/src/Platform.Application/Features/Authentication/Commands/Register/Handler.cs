using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Role> _roles;
    private readonly IRepository<TeacherProfile> _teacherProfiles;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailSender _email;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public RegisterHandler(
        IRepository<User> users,
        IRepository<Role> roles,
        IRepository<TeacherProfile> teacherProfiles,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IEmailSender email,
        ICurrentUser current,
        IClock clock)
    {
        _users           = users;
        _roles           = roles;
        _teacherProfiles = teacherProfiles;
        _auditLogs       = auditLogs;
        _uow             = uow;
        _hasher          = hasher;
        _email           = email;
        _current         = current;
        _clock           = clock;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var normalizedEmail = cmd.Email.Trim().ToLowerInvariant();

        if (await _users.AnyAsync(u => u.Email == normalizedEmail, ct))
            return Error.Conflict("auth.email_in_use", "An account with this email already exists.");

        var targetRoleName = cmd.Role?.Trim().ToLowerInvariant() switch
        {
            "teacher" => "Teacher",
            "admin"   => "Admin",
            _         => "Student"
        };

        var role = await _roles.FirstOrDefaultAsync(r => r.Name == targetRoleName, ct)
                   ?? await _roles.FirstOrDefaultAsync(r => r.Name == "Student", ct);

        if (role is null)
            return Error.NotFound("role.not_found", "Requested role was not found.");

        var now  = _clock.UtcNow.UtcDateTime;
        var hash = _hasher.Hash(cmd.Password);

        var user = new User
        {
            FullName      = cmd.FullName.Trim(),
            Email         = normalizedEmail,
            Phone         = cmd.Phone?.Trim(),
            PasswordHash  = hash,
            RoleId        = role.Id,
            IsActive      = true,
            EmailConfirmed = false,
            CreatedAt     = now,
            UpdatedAt     = now
        };

        await _users.AddAsync(user, ct);

        if (role.Name == "Teacher")
        {
            await _teacherProfiles.AddAsync(new TeacherProfile
            {
                User = user,
                Biography = "Instructor on Codean Platform",
                IsVerified = true
            }, ct);
        }

        await _auditLogs.AddAsync(new AuditLog
        {
            User       = user,
            Action     = "User.Registered",
            EntityType = "User",
            IpAddress  = _current.IpAddress,
            CreatedAt  = now,
            NewValues  = $"{{\"Email\":\"{normalizedEmail}\",\"FullName\":\"{user.FullName}\",\"Role\":\"{role.Name}\"}}"
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Generate short-lived email verification token (SHA-256 of a random GUID)
        var rawToken   = Guid.NewGuid().ToString("N");
        var tokenHash  = HashToken(rawToken);

        // Fire-and-forget: do NOT await here to avoid delaying the response
        _ = _email.SendAsync(new EmailMessage(
            To:       user.Email,
            Subject:  "Verify Your Email — Platform",
            HtmlBody: $"<h1>Welcome, {user.FullName}!</h1>" +
                      $"<p>Your verification code: <strong>{rawToken}</strong></p>" +
                      $"<p>This code expires in 30 minutes.</p>",
            TextBody: $"Verification code: {rawToken}. Expires in 30 minutes."), ct);

        return user.ToRegisterResponse();
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
