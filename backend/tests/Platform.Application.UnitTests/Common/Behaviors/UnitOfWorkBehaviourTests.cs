using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Behaviors;
using Xunit;

namespace Platform.Application.UnitTests.Common.Behaviors;

public class UnitOfWorkBehaviourTests
{
    public record TestQuery : IRequest<string>;
    public record TestCommand : IRequest<string>;

    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IDbTransaction _tx = Substitute.For<IDbTransaction>();

    public UnitOfWorkBehaviourTests()
    {
        _uow.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_tx);
    }

    [Fact]
    public async Task Handle_WhenRequestIsQuery_ShouldNotOpenTransaction()
    {
        // Arrange
        var behavior = new UnitOfWorkBehaviour<TestQuery, string>(_uow);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        // Act
        var result = await behavior.Handle(new TestQuery(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
        await _uow.DidNotReceiveWithAnyArgs().BeginTransactionAsync(default!);
    }

    [Fact]
    public async Task Handle_WhenRequestIsCommand_ShouldCommitTransaction()
    {
        // Arrange
        var behavior = new UnitOfWorkBehaviour<TestCommand, string>(_uow);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");

        await _uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandThrows_ShouldRollbackTransaction()
    {
        // Arrange
        var behavior = new UnitOfWorkBehaviour<TestCommand, string>(_uow);
        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("Failed");

        // Act
        Func<Task> act = async () => await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
