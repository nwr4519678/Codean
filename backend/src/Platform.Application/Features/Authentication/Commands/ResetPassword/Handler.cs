using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public ResetPasswordHandler(
        IRepository<User> users,
        IRepository<AuditLog> auditLogs,
        IRepository<Domain.Entities.RefreshToken> refreshTokens,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ICurrentUser current,
        IClock clock)
    {
        _users         = users;
        _auditLogs     = auditLogs;
        _refreshTokens = refreshTokens;
        _uow           = uow;
        _hasher        = hasher;
        _current       = current;
        _clock         = clock;
    }

    public async Task<Result> Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var normalized = cmd.Email.Trim().ToLowerInvariant();
        var user       = await _users.FirstOrDefaultAsync(u => u.Email == normalized, ct);

        if (user is null || !user.IsActive)
            return Error.Failure("auth.reset_invalid", "Invalid or expired password reset token.");

        var now        = _clock.UtcNow.UtcDateTime;
        var tokenHash  = HashToken(cmd.Token);

        // Locate the single-use reset log created by ForgotPasswordHandler
        var resetLog = await _auditLogs.FirstOrDefaultAsync(
            l => l.UserId == user.Id &&
                 l.Action == "User.PasswordResetRequested" &&
                 l.NewValues != null &&
                 l.NewValues.Contains(tokenHash), ct);

        if (resetLog is null)
            return Error.Failure("auth.reset_invalid", "Invalid or expired password reset token.");

        if (resetLog.CreatedAt.AddMinutes(30) < now)
            return Error.Failure("auth.reset_expired", "Password reset token has expired. Please request a new one.");

        // Mutate password
        user.PasswordHash = _hasher.Hash(cmd.NewPassword);
        user.UpdatedAt    = now;
        _users.Update(user);

        // Consume the token — prevent reuse
        resetLog.NewValues = $"{{\"TokenHash\":\"CONSUMED\",\"ConsumedAt\":\"{now:O}\"}}";
        _auditLogs.Update(resetLog);

        // Revoke all active refresh tokens
        var activeTokens = await _refreshTokens.ListAsync(
            t => t.UserId == user.Id && t.RevokedAt == null, ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt       = now;
            token.RevokedByIp     = _current.IpAddress;
            token.ReplacedByToken = "PASSWORD_RESET";
            _refreshTokens.Update(token);
        }

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = user.Id,
            Action     = "User.PasswordReset",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
