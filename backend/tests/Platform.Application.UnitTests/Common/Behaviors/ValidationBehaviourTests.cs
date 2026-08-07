using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Platform.Application.Common.Behaviors;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviourTests
{
    public record TestRequest : IRequest<Result>;

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehaviour<TestRequest, Result>(Enumerable.Empty<IValidator<TestRequest>>());
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () => { nextCalled = true; return Task.FromResult(Result.Success()); };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorsPass_ShouldCallNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                 .Returns(new ValidationResult());

        var behavior = new ValidationBehaviour<TestRequest, Result>(new[] { validator });
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () => { nextCalled = true; return Task.FromResult(Result.Success()); };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldReturnValidationFailureResultWithoutCallingNext()
    {
        // Arrange
        var failure = new ValidationFailure("PropName", "Error message");
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                 .Returns(new ValidationResult(new[] { failure }));

        var behavior = new ValidationBehaviour<TestRequest, Result>(new[] { validator });
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () => { nextCalled = true; return Task.FromResult(Result.Success()); };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("validation.failed");
        result.Error.Metadata.Should().ContainKey("PropName");
    }
}
