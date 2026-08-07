using System;
using FluentAssertions;
using Platform.Domain.Primitives;
using Xunit;

namespace Platform.Domain.UnitTests.Primitives;

public class AggregateRootTests
{
    private record TestDomainEvent(string Data) : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    private class TestAggregate : AggregateRoot
    {
        public void DoSomething(string data)
        {
            RaiseDomainEvent(new TestDomainEvent(data));
        }

        public void RaiseNullEvent()
        {
            RaiseDomainEvent(null!);
        }
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var agg = new TestAggregate();

        // Act
        agg.DoSomething("event1");

        // Assert
        agg.DomainEvents.Should().HaveCount(1);
        agg.DomainEvents[0].Should().BeOfType<TestDomainEvent>()
           .Which.Data.Should().Be("event1");
    }

    [Fact]
    public void RaiseDomainEvent_WhenNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var agg = new TestAggregate();

        // Act
        Action act = () => agg.RaiseNullEvent();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MultipleEvents_ShouldBeAccumulatedInOrder()
    {
        // Arrange
        var agg = new TestAggregate();

        // Act
        agg.DoSomething("first");
        agg.DoSomething("second");

        // Assert
        agg.DomainEvents.Should().HaveCount(2);
        ((TestDomainEvent)agg.DomainEvents[0]).Data.Should().Be("first");
        ((TestDomainEvent)agg.DomainEvents[1]).Data.Should().Be("second");
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyCollection()
    {
        // Arrange
        var agg = new TestAggregate();
        agg.DoSomething("event");

        // Act
        agg.ClearDomainEvents();

        // Assert
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void PopDomainEvents_ShouldReturnEventsAndClearCollection()
    {
        // Arrange
        var agg = new TestAggregate();
        agg.DoSomething("event1");
        agg.DoSomething("event2");

        // Act
        var popped = agg.PopDomainEvents();

        // Assert
        popped.Should().HaveCount(2);
        agg.DomainEvents.Should().BeEmpty();
    }
}
