using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Platform.Api.Authorization;
using Xunit;

namespace Platform.Api.UnitTests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _sut = new();

    [Fact]
    public async Task HandleAsync_WhenUserHasPermissionClaim_ShouldSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("courses.read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "courses.read")
        }, "TestAuth"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserLacksPermissionClaim_ShouldNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("courses.write");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "courses.read")
        }, "TestAuth"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
