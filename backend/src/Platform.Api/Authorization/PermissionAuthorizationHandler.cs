using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Platform.Domain.Security;

namespace Platform.Api.Authorization;

/// <summary>
/// Authorization requirement that checks for a specific permission claim in the JWT.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Validates that the authenticated user holds the required permission claim.
/// Checked via JwtBearerEvents before reaching the controller.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasClaim = context.User.HasClaim(c =>
            c.Type == "permission" && c.Value == requirement.Permission);

        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
