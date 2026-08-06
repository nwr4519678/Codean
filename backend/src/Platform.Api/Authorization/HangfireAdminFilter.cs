using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Platform.Api.Authorization;

/// <summary>
/// Restricts the Hangfire dashboard to authenticated users with the Admin role.
/// Registered in Program.cs via DashboardOptions.Authorization.
/// </summary>
public sealed class HangfireAdminFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Must be authenticated
        if (httpContext.User.Identity?.IsAuthenticated != true)
            return false;

        // Must hold the Admin role
        return httpContext.User.IsInRole("Admin");
    }
}
