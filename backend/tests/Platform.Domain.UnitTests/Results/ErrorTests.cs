using System.Collections.Generic;
using FluentAssertions;
using Platform.Domain.Results;
using Xunit;

namespace Platform.Domain.UnitTests.Results;

public class ErrorTests
{
    [Fact]
    public void None_ShouldHaveEmptyCodeAndMessage()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Failure_ShouldSetCodeMessageAndType()
    {
        var err = Error.Failure("code.failure", "Failed");
        err.Code.Should().Be("code.failure");
        err.Message.Should().Be("Failed");
        err.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Validation_ShouldSetTypeAndMetadata()
    {
        var meta = new Dictionary<string, object?> { { "field", "invalid" } };
        var err = Error.Validation("code.val", "Validation failed", meta);

        err.Code.Should().Be("code.val");
        err.Message.Should().Be("Validation failed");
        err.Type.Should().Be(ErrorType.Validation);
        err.Metadata.Should().BeEquivalentTo(meta);
    }

    [Fact]
    public void NotFound_ShouldSetTypeNotFound()
    {
        var err = Error.NotFound("code.not_found", "Not found");
        err.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_ShouldSetTypeConflict()
    {
        var err = Error.Conflict("code.conflict", "Conflict");
        err.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Forbidden_ShouldSetTypeForbidden()
    {
        var err = Error.Forbidden("code.forbidden", "Forbidden");
        err.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public void Unauthorized_ShouldSetTypeUnauthorized()
    {
        var err = Error.Unauthorized("code.unauthorized", "Unauthorized");
        err.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public void SubscriptionRequired_ShouldSetTypeSubscriptionRequired()
    {
        var err = Error.SubscriptionRequired("code.sub", "Sub required");
        err.Type.Should().Be(ErrorType.SubscriptionRequired);
    }

    [Fact]
    public void Provider_ShouldSetTypeProvider()
    {
        var err = Error.Provider("code.provider", "Provider error");
        err.Type.Should().Be(ErrorType.Provider);
    }

    [Fact]
    public void Internal_ShouldSetTypeInternal()
    {
        var err = Error.Internal("code.internal", "Internal error");
        err.Type.Should().Be(ErrorType.Internal);
    }
}
