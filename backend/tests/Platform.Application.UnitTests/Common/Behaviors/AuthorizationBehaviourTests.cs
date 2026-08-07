using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Behaviors;
using Platform.Application.Common.Security;
using Platform.Domain.Exceptions;
using Xunit;

namespace Platform.Application.UnitTests.Common.Behaviors;

public class AuthorizationBehaviourTests
{
    public record AnonymousRequest : IRequest<string>;

    [Authorize]
    public record SecuredRequest : IRequest<string>;

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_UnsecuredRequest_ShouldCallNextRegardlessOfAuthStatus()
    {
        // Arrange
        _currentUser.UserId.Returns((long?)null);
        var behavior = new AuthorizationBehaviour<AnonymousRequest, string>(_currentUser);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        // Act
        var result = await behavior.Handle(new AnonymousRequest(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_SecuredRequest_WhenUserNotAuthenticated_ShouldThrowDomainException()
    {
        // Arrange
        _currentUser.UserId.Returns((long?)null);
        var behavior = new AuthorizationBehaviour<SecuredRequest, string>(_currentUser);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        // Act
        Func<Task> act = async () => await behavior.Handle(new SecuredRequest(), next, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Error.Code.Should().Be("auth.unauthorized");
    }

    [Fact]
    public async Task Handle_SecuredRequest_WhenUserAuthenticated_ShouldCallNext()
    {
        // Arrange
        _currentUser.UserId.Returns(10L);
        var behavior = new AuthorizationBehaviour<SecuredRequest, string>(_currentUser);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        // Act
        var result = await behavior.Handle(new SecuredRequest(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }
}
