using System;

namespace Platform.Domain.Entities;

/// <summary>
/// Extends the auto-generated User entity with account lockout fields.
/// This partial class is separate from the EF Core Power Tools generated User.cs
/// to preserve changes across re-scaffolding cycles.
///
/// Corresponding migration: AddUserLockoutFields
/// Adds columns: failed_login_count (int, default 0), lockout_end (timestamptz, nullable)
/// </summary>
public partial class User
{
    /// <summary>
    /// Number of consecutive failed login attempts since the last successful login.
    /// Reset to 0 on successful login.
    /// </summary>
    public int FailedLoginCount { get; set; }

    /// <summary>
    /// UTC datetime until which this account is locked.
    /// Null means the account is not locked.
    /// Set when FailedLoginCount reaches LockoutSettings.MaxFailedAttempts.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
}
