using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Platform.Api.Controllers;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Api.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Register_WhenSuccess_ShouldReturn201Created()
    {
        // Arrange
        var response = new RegisterResponse(1, "user@test.com", "John Doe", true);
        _sender.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
               .Returns(Result<RegisterResponse>.Success(response));

        var cmd = new RegisterCommand("John Doe", "user@test.com", "Pass123!");

        // Act
        var result = await _sut.Register(cmd, CancellationToken.None);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        created.Value.Should().Be(response);
    }

    [Fact]
    public async Task Register_WhenConflictError_ShouldReturn409Conflict()
    {
        // Arrange
        var error = Error.Conflict("auth.email_in_use", "Already exists");
        _sender.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
               .Returns(Result<RegisterResponse>.Failure(error));

        var cmd = new RegisterCommand("John Doe", "user@test.com", "Pass123!");

        // Act
        var result = await _sut.Register(cmd, CancellationToken.None);

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Login_WhenSuccess_ShouldReturn200Ok()
    {
        // Arrange
        var response = new LoginResponse(1, "user@test.com", "John Doe", "Student", "access", "refresh", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
               .Returns(Result<LoginResponse>.Success(response));

        var cmd = new LoginCommand("user@test.com", "Pass123!");

        // Act
        var result = await _sut.Login(cmd, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_WhenUnauthorized_ShouldReturn401Unauthorized()
    {
        // Arrange
        var error = Error.Unauthorized("auth.invalid_credentials", "Invalid credentials");
        _sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
               .Returns(Result<LoginResponse>.Failure(error));

        var cmd = new LoginCommand("user@test.com", "Wrong!");

        // Act
        var result = await _sut.Login(cmd, CancellationToken.None);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RevokeToken_WhenSuccess_ShouldReturn24NoContent()
    {
        // Arrange
        _sender.Send(Arg.Any<RevokeTokenCommand>(), Arg.Any<CancellationToken>())
               .Returns(Result.Success());

        var cmd = new RevokeTokenCommand("token");

        // Act
        var result = await _sut.RevokeToken(cmd, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserClaimMissing_ShouldReturn401Unauthorized()
    {
        // Act
        var result = await _sut.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserClaimPresent_ShouldReturn200Ok()
    {
        // Arrange
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", "10")
        }, "TestAuth"));

        _sut.ControllerContext.HttpContext.User = userClaims;

        var profile = new CurrentUserResponse(10, "user@test.com", "User", null, "Student", true, null, DateTime.UtcNow);
        _sender.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
               .Returns(Result<CurrentUserResponse>.Success(profile));

        // Act
        var result = await _sut.GetCurrentUser(CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(profile);
    }

    [Fact]
    public async Task GetCurrentUser_WhenExternalSubjectAndPlatformUserIdPresent_ShouldUsePlatformUserId()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user_clerk123"),
            new Claim("platform_user_id", "10")
        }, "TestAuth"));

        _sut.ControllerContext.HttpContext.User = userClaims;
        var profile = new CurrentUserResponse(10, "user@test.com", "User", null, "Student", true, null, DateTime.UtcNow);
        _sender.Send(Arg.Is<GetCurrentUserQuery>(query => query.UserId == 10), Arg.Any<CancellationToken>())
               .Returns(Result<CurrentUserResponse>.Success(profile));

        var result = await _sut.GetCurrentUser(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(profile);
    }
}
