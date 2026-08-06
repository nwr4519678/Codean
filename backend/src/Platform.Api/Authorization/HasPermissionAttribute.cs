using System;
using Microsoft.AspNetCore.Authorization;

namespace Platform.Api.Authorization;

/// <summary>
/// Applies a permission-based policy using the dynamic PermissionPolicyProvider.
/// Usage: [HasPermission(Permissions.UsersRead)]
/// Lives in Platform.Api since it depends on ASP.NET Core AuthorizeAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base($"permission:{permission}")
    {
    }
}
