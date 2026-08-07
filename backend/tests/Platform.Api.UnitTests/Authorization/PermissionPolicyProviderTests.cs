using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Platform.Api.Authorization;
using Xunit;

namespace Platform.Api.UnitTests.Authorization;

public class PermissionPolicyProviderTests
{
    private readonly PermissionPolicyProvider _sut;

    public PermissionPolicyProviderTests()
    {
        var options = Options.Create(new AuthorizationOptions());
        _sut = new PermissionPolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPermissionPrefix_ShouldBuildPolicyWithRequirement()
    {
        // Act
        var policy = await _sut.GetPolicyAsync("permission:courses.read");

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements.OfType<PermissionRequirement>().Should().ContainSingle(pr => pr.Permission == "courses.read");
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ShouldReturnDefaultPolicy()
    {
        // Act
        var policy = await _sut.GetDefaultPolicyAsync();

        // Assert
        policy.Should().NotBeNull();
    }
}
