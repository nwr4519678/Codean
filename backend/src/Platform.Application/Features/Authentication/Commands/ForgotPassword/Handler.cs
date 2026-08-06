using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _email;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public ForgotPasswordHandler(
        IRepository<User> users,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        IEmailSender email,
        ICurrentUser current,
        IClock clock)
    {
        _users     = users;
        _auditLogs = auditLogs;
        _uow       = uow;
        _email     = email;
        _current   = current;
        _clock     = clock;
    }

    public async Task<Result> Handle(ForgotPasswordCommand cmd, CancellationToken ct)
    {
        var normalized = cmd.Email.Trim().ToLowerInvariant();
        var user       = await _users.FirstOrDefaultAsync(u => u.Email == normalized, ct);

        // Anti-enumeration: always return success regardless of whether the user exists
        if (user is null || !user.IsActive)
            return Result.Success();

        var now      = _clock.UtcNow.UtcDateTime;
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash     = HashToken(rawToken);

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = user.Id,
            Action     = "User.PasswordResetRequested",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now,
            // Store hash + expiry — single-use checked by ResetPasswordHandler
            NewValues  = $"{{\"TokenHash\":\"{hash}\",\"ExpiresAt\":\"{now.AddMinutes(30):O}\"}}"
        }, ct);

        await _uow.SaveChangesAsync(ct);

        _ = _email.SendAsync(new EmailMessage(
            To:       user.Email,
            Subject:  "Password Reset Request — Platform",
            HtmlBody: $"<h1>Password Reset</h1><p>Token: <strong>{rawToken}</strong></p>" +
                      $"<p>Expires in 30 minutes. <strong>Do not share this token.</strong></p>",
            TextBody: $"Password reset token: {rawToken}. Expires in 30 min."), ct);

        return Result.Success();
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
