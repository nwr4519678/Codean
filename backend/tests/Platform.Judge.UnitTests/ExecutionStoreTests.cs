using System;
using FluentAssertions;
using Platform.Judge.Contracts;
using Platform.Judge.Services;
using Xunit;

namespace Platform.Judge.UnitTests;

public class ExecutionStoreTests
{
    private readonly ExecutionStore _store = new();

    [Fact]
    public void TryAddOrGetSubmission_ShouldHandleIdempotency()
    {
        var execId = "exec-123";
        var subId = 456L;

        bool isFirstCall = _store.TryAddOrGetSubmission(execId, subId, out var firstStatus);
        bool isSecondCall = _store.TryAddOrGetSubmission(execId, subId, out var secondStatus);

        isFirstCall.Should().BeTrue();
        isSecondCall.Should().BeFalse();
        firstStatus.ExecutionId.Should().Be(execId);
        secondStatus.ExecutionId.Should().Be(execId);
        firstStatus.Status.Should().Be(ExecutionStatus.Queued);
    }
}
