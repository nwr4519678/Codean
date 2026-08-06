using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Commands.TwoFactor;

// ── Setup 2FA Handler ─────────────────────────────────────────────────────────

public sealed class SetupTwoFactorHandler : IRequestHandler<SetupTwoFactorCommand, Result<SetupTwoFactorResponse>>
{
    private readonly IRepository<User> _users;
    private readonly ITotpService _totp;
    private readonly ICurrentUser _current;

    public SetupTwoFactorHandler(
        IRepository<User> users,
        ITotpService totp,
        ICurrentUser current)
    {
        _users   = users;
        _totp    = totp;
        _current = current;
    }

    public async Task<Result<SetupTwoFactorResponse>> Handle(SetupTwoFactorCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        var user = await _users.GetByIdAsync(_current.UserId.Value, ct);
        if (user is null)
            return Error.NotFound("auth.user_not_found", "User not found.");

        var secret      = _totp.GenerateSecret();
        var qrUri       = _totp.GenerateQrUri(user.Email, secret);
        var backupCodes = _totp.GenerateBackupCodes(10);

        return AuthenticationMappingExtensions.ToSetupTwoFactorResponse(secret, qrUri, backupCodes);
    }
}

// ── Verify & Enable 2FA Handler ───────────────────────────────────────────────

public sealed class VerifyTwoFactorHandler : IRequestHandler<VerifyTwoFactorCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IRepository<AuditLog> _auditLogs;
    private readonly IUnitOfWork _uow;
    private readonly ITotpService _totp;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public VerifyTwoFactorHandler(
        IRepository<User> users,
        IRepository<AuditLog> auditLogs,
        IUnitOfWork uow,
        ITotpService totp,
        ICurrentUser current,
        IClock clock)
    {
        _users     = users;
        _auditLogs = auditLogs;
        _uow       = uow;
        _totp      = totp;
        _current   = current;
        _clock     = clock;
    }

    public async Task<Result> Handle(VerifyTwoFactorCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        if (!_totp.Verify(cmd.Secret, cmd.Code))
            return Error.Failure("auth.2fa_invalid_code", "Invalid TOTP code. Please check your authenticator and try again.");

        var user = await _users.GetByIdAsync(_current.UserId.Value, ct);
        if (user is null)
            return Error.NotFound("auth.user_not_found", "User not found.");

        var now = _clock.UtcNow.UtcDateTime;

        await _auditLogs.AddAsync(new AuditLog
        {
            UserId     = user.Id,
            Action     = "User.TwoFactorEnabled",
            EntityType = "User",
            EntityId   = user.Id,
            IpAddress  = _current.IpAddress,
            CreatedAt  = now
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
