using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Results;
using Swashbuckle.AspNetCore.Annotations;
using Error = Platform.Domain.Results.Error;
using ErrorType = Platform.Domain.Results.ErrorType;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Api.Controllers;

/// <summary>
/// Handles all authentication and account security operations.
/// </summary>
[Route("api/auth")]
public sealed class AuthController : ApiController
{
    private readonly ISender _sender;
    private readonly AppDbContext? _db;

    public AuthController(ISender sender, AppDbContext? db = null) { _sender = sender; _db = db; }

    /// <summary>Synchronizes a Supabase Auth identity with the application profile.</summary>
    [HttpPost("sync-profile")]
    [Authorize]
    public async Task<IActionResult> SyncProfile([FromBody] SyncProfileRequest request, CancellationToken ct)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        if (_db is null) return StatusCode(500, new ProblemDetails { Title = "Database unavailable" });
        email = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user is null) return NotFound(new ProblemDetails { Title = "Profile not found", Detail = "The authenticated profile could not be provisioned." });
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return Ok(new { userId = user.Id, email = user.Email, fullName = user.FullName });
    }

    // ── Registration ─────────────────────────────────────────────────────────

    /// <summary>Registers a new user account and sends an email verification link.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_register")]
    [SwaggerOperation(Summary = "Register", Tags = ["Auth"])]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand cmd, CancellationToken ct)
    {
        // Public registration is always Student — Teachers are created by admins only.
        var studentCmd = cmd with { Role = "Student" };
        var result = await _sender.Send(studentCmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentUser), null, result.Value)
            : MapError(result.Error);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    /// <summary>Authenticates a user and returns JWT access + refresh tokens.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_login")]
    [SwaggerOperation(Summary = "Login", Tags = ["Auth"])]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    // ── Token Refresh ─────────────────────────────────────────────────────────

    /// <summary>Issues a new access token using a valid refresh token (automatic rotation).</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Refresh Token", Tags = ["Auth"])]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    // ── Logout / Revoke Token ─────────────────────────────────────────────────

    /// <summary>Revokes a refresh token, ending the current session.</summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [SwaggerOperation(Summary = "Logout / Revoke Token", Tags = ["Auth"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Current User ──────────────────────────────────────────────────────────

    /// <summary>Returns the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get Current User", Tags = ["Auth"])]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(Problem(Error.Unauthorized("auth.unauthenticated", "Invalid token."), 401));

        var result = await _sender.Send(new GetCurrentUserQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    // ── Change Password ───────────────────────────────────────────────────────

    /// <summary>Changes the authenticated user's password. Revokes all other active sessions.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("auth_sensitive")]
    [SwaggerOperation(Summary = "Change Password", Tags = ["Auth"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Forgot / Reset Password ───────────────────────────────────────────────

    /// <summary>Sends a password reset email. Always returns 204 to prevent user enumeration.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_sensitive")]
    [SwaggerOperation(Summary = "Forgot Password", Tags = ["Auth"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd, CancellationToken ct)
    {
        await _sender.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>Resets the user's password with a valid single-use token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_sensitive")]
    [SwaggerOperation(Summary = "Reset Password", Tags = ["Auth"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Email Verification ────────────────────────────────────────────────────

    /// <summary>Verifies the user's email address using a one-time token.</summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_sensitive")]
    [SwaggerOperation(Summary = "Verify Email", Tags = ["Auth"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Two-Factor Authentication ─────────────────────────────────────────────

    /// <summary>Initiates 2FA setup. Returns a TOTP secret, QR code URI, and backup codes.</summary>
    [HttpPost("2fa/setup")]
    [Authorize]
    [SwaggerOperation(Summary = "Setup 2FA", Tags = ["Auth — 2FA"])]
    [ProducesResponseType(typeof(SetupTwoFactorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetupTwoFactor(CancellationToken ct)
    {
        var result = await _sender.Send(new SetupTwoFactorCommand(), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Confirms and activates 2FA by verifying the TOTP code from an authenticator app.</summary>
    [HttpPost("2fa/verify")]
    [Authorize]
    [SwaggerOperation(Summary = "Verify & Enable 2FA", Tags = ["Auth — 2FA"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    // ── Sessions ──────────────────────────────────────────────────────────────

    /// <summary>Returns all sessions (active and historical) for the authenticated user.</summary>
    [HttpGet("sessions")]
    [Authorize]
    [SwaggerOperation(Summary = "Get Sessions", Tags = ["Auth — Sessions"])]
    [ProducesResponseType(typeof(List<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(Problem(Error.Unauthorized("auth.unauthenticated", "Invalid token."), 401));

        var result = await _sender.Send(new GetActiveSessionsQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Revokes a specific session by its ID. Cannot revoke other users' sessions.</summary>
    [HttpDelete("sessions/{sessionId:long}")]
    [Authorize]
    [SwaggerOperation(Summary = "Revoke Session", Tags = ["Auth — Sessions"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession([FromRoute] long sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new RevokeSessionCommand(sessionId), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

}

public sealed record SyncProfileRequest(string? FullName);
