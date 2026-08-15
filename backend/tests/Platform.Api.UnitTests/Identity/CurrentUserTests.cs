using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Platform.Infrastructure.Identity;

namespace Platform.Api.UnitTests.Identity;

public sealed class CurrentUserTests
{
    [Fact]
    public void UserId_ShouldPreferPlatformUserIdOverExternalSubject()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user_clerk123"),
                    new Claim("platform_user_id", "42"),
                }, "TestAuth")),
            },
        };

        var currentUser = new CurrentUser(accessor);

        currentUser.UserId.Should().Be(42);
    }
}
