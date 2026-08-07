using System;
using System.Threading.Tasks;
using FluentAssertions;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Domain.UnitTests.Results;

public class ResultTests
{
    [Fact]
    public void Success_Generic_ShouldCreateSuccessfulResultWithValue()
    {
        // Act
        var result = Result<string>.Success("test_value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test_value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Generic_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var error = Error.NotFound("code", "message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitOperator_Value_ShouldCreateSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitOperator_Error_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Conflict("code", "message");

        // Act
        Result<int> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Map_WhenSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var mapped = result.Map(val => val * 2);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(20);
    }

    [Fact]
    public void Map_WhenFailure_ShouldPropagateError()
    {
        // Arrange
        var error = Error.Validation("code", "message");
        var result = Result<int>.Failure(error);

        // Act
        var mapped = result.Map(val => val * 2);

        // Assert
        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public async Task BindAsync_WhenSuccess_ShouldExecuteBinder()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var bound = await result.BindAsync(val => Task.FromResult(Result<string>.Success($"val_{val}")));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("val_5");
    }

    [Fact]
    public async Task BindAsync_WhenFailure_ShouldShortCircuit()
    {
        // Arrange
        var error = Error.Unauthorized("code", "message");
        var result = Result<int>.Failure(error);
        var binderCalled = false;

        // Act
        var bound = await result.BindAsync(val =>
        {
            binderCalled = true;
            return Task.FromResult(Result<string>.Success("should not be called"));
        });

        // Assert
        binderCalled.Should().BeFalse();
        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be(error);
    }

    [Fact]
    public void NonGeneric_Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.Forbidden("code", "message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void NonGeneric_ImplicitOperator_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Internal("code", "message");

        // Act
        Result result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }
}
