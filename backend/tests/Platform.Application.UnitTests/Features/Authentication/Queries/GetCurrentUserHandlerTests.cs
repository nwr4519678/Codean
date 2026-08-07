using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Queries.GetCurrentUser;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Queries;

public class GetCurrentUserHandlerTests
{
    private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly GetCurrentUserHandler _sut;

    public GetCurrentUserHandlerTests()
    {
        _sut = new GetCurrentUserHandler(_users, _current);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _users.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns((User?)null);
        var query = new GetCurrentUserQuery(10L);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("auth.user_not_found");
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnCurrentUserResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 10L,
            Email = "student@example.com",
            FullName = "Jane Doe",
            Phone = "123456",
            RoleId = 1,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.GetByIdAsync(10L, Arg.Any<CancellationToken>()).Returns(user);
        var query = new GetCurrentUserQuery(10L);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(10L);
        result.Value.Email.Should().Be("student@example.com");
        result.Value.FullName.Should().Be("Jane Doe");
        result.Value.Role.Should().Be("Student");
    }
}
