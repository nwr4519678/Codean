using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Api.Authorization;
using Platform.Application.Common.Pagination;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Users.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Api.Controllers;

/// <summary>
/// Administrative user management — search, status, and role assignment.
/// Requires the users.manage permission (Admin only).
/// </summary>
[Route("api/users")]
[Authorize]
public sealed class UsersController : ApiController
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

    /// <summary>Returns a paged list of users with search and filter parameters.</summary>
    [HttpGet]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Get Users Paged", Tags = ["Users"])]
    [ProducesResponseType(typeof(PagedList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersPagedQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Returns user details by user ID.</summary>
    [HttpGet("{id:long}")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Get User By ID", Tags = ["Users"])]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    /// <summary>Activates or deactivates a user account.</summary>
    [HttpPut("{id:long}/status")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Set User Status", Tags = ["Users"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUserStatus(
        [FromRoute] long id,
        [FromBody] SetUserStatusRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new SetUserStatusCommand(id, request.IsActive), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Assigns a new role to a user.</summary>
    [HttpPut("{id:long}/role")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Assign User Role", Tags = ["Users"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        [FromRoute] long id,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new AssignUserRoleCommand(id, request.RoleId), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Deletes a user account (admin only).</summary>
    [HttpDelete("{id:long}")]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Delete User", Tags = ["Users"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] long id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteUserCommand(id), ct);
        return result.IsSuccess ? NoContent() : MapError(result.Error);
    }

    /// <summary>Creates a new user account (admin only). Bypasses the public registration rate limiter.</summary>
    [HttpPost]
    [HasPermission("users.manage")]
    [SwaggerOperation(Summary = "Admin Create User", Tags = ["Users"])]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken ct)
    {
        var cmd = new RegisterCommand(
            FullName: $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim(),
            Email:    request.Email,
            Password: request.Password,
            Role:     "Teacher");

        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetUserById), new { id = result.Value!.UserId }, result.Value)
            : MapError(result.Error);
    }
}

public sealed record SetUserStatusRequest(bool IsActive);
public sealed record AssignRoleRequest(int RoleId);

/// <summary>Request body for admin-created user accounts.</summary>
public sealed record AdminCreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
