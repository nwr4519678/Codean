using Platform.Domain.Primitives;

namespace Platform.Domain.Events.Authentication;

/// <summary>
/// Raised when a new user completes registration.
/// Triggers: welcome email, initial profile setup.
/// </summary>
public sealed record UserRegisteredEvent(
    Guid Id,
    DateTime OccurredOn,
    long UserId,
    string Email,
    string FullName,
    string? CorrelationId = null) : IDomainEvent;

/// <summary>
/// Raised when a user successfully verifies their email address.
/// Triggers: confirmation email, unlock full account access.
/// </summary>
public sealed record EmailVerifiedEvent(
    Guid Id,
    DateTime OccurredOn,
    long UserId,
    string Email,
    string? CorrelationId = null) : IDomainEvent;

/// <summary>
/// Raised when a user changes their password.
/// Triggers: security notification email, revoke all active sessions.
/// </summary>
public sealed record PasswordChangedEvent(
    Guid Id,
    DateTime OccurredOn,
    long UserId,
    string Email,
    string? CorrelationId = null) : IDomainEvent;

/// <summary>
/// Raised on every successful login.
/// Triggers: suspicious login alert (new IP), audit log enrichment.
/// </summary>
public sealed record UserLoggedInEvent(
    Guid Id,
    DateTime OccurredOn,
    long UserId,
    string Email,
    string? IpAddress,
    string? DeviceInfo,
    string? CorrelationId = null) : IDomainEvent;
