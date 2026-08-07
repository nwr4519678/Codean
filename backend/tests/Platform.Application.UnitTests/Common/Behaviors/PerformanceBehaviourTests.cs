using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Platform.Application.Common.Behaviors;
using Xunit;

namespace Platform.Application.UnitTests.Common.Behaviors;

public class PerformanceBehaviourTests
{
    public record TestRequest : IRequest<string>;

    private readonly ILogger<PerformanceBehaviour<TestRequest, string>> _logger = Substitute.For<ILogger<PerformanceBehaviour<TestRequest, string>>>();

    [Fact]
    public async Task Handle_FastRequest_ShouldCallNextWithoutWarning()
    {
        // Arrange
        var behavior = new PerformanceBehaviour<TestRequest, string>(_logger);
        RequestHandlerDelegate<string> next = () => Task.FromResult("fast");

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        result.Should().Be("fast");
    }

    [Fact]
    public async Task Handle_SlowRequest_ShouldLogWarning()
    {
        // Arrange
        var behavior = new PerformanceBehaviour<TestRequest, string>(_logger);
        RequestHandlerDelegate<string> next = async () =>
        {
            await Task.Delay(550);
            return "slow";
        };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        result.Should().Be("slow");
        _logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<System.Exception>(),
            Arg.Any<System.Func<object, System.Exception?, string>>());
    }
}
