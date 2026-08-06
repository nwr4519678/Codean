using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Platform.Api.Authorization;

/// <summary>
/// Dynamically creates authorization policies named after permissions (e.g. "permission:users.read").
/// This avoids manually registering every permission constant as a named policy.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPolicyPrefix = "permission:";
    private readonly AuthorizationOptions _options;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _options = options.Value;
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PermissionPolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to static policies registered via AddAuthorization
        return Task.FromResult(_options.GetPolicy(policyName));
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        Task.FromResult(_options.DefaultPolicy ?? new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        Task.FromResult(_options.FallbackPolicy);
}
