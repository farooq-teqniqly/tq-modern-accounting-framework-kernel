namespace Teqniqly.MAF.Kernel.Domain.Tests;

public class AggregateRootTests
{
    [Fact]
    public void WhenCreated_GeneratesNonEmptyId()
    {
        // Arrange & Act
        var aggregate = new TestAggregate();

        // Assert
        Assert.NotEqual(Guid.Empty, aggregate.Id);
    }

    [Fact]
    public void WhenCreated_GeneratesVersion7Guid()
    {
        // Arrange & Act
        var aggregate = new TestAggregate();

        // Assert - Version 7 GUIDs have version bits set to 0111 (7) in byte 7
        var guidBytes = aggregate.Id.ToByteArray();
        var version = (guidBytes[7] & 0xF0) >> 4;
        Assert.Equal(7, version);
    }

    [Fact]
    public void WhenCreated_HasEmptyDomainEvents()
    {
        // Arrange & Act
        var aggregate = new TestAggregate();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void WhenCreatedWithId_UsesProvidedId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var aggregate = new TestAggregate(expectedId);

        // Assert
        Assert.Equal(expectedId, aggregate.Id);
    }

    [Fact]
    public void WhenDomainEventAdded_ContainsEvent()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void WhenDomainEventsCleared_IsEmpty()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.AddDomainEvent(new TestDomainEvent());
        aggregate.AddDomainEvent(new TestDomainEvent());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void WhenMultipleDomainEventsAdded_ContainsAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Act
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Assert
        Assert.Equal(2, aggregate.DomainEvents.Count);
        Assert.Contains(event1, aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
    }

    // Minimal test aggregate to test the abstract base class
    private sealed class TestAggregate : AggregateRoot
    {
        public TestAggregate() { }

        public TestAggregate(Guid id)
            : base(id) { }

        public new void AddDomainEvent(IDomainEvent domainEvent) =>
            base.AddDomainEvent(domainEvent);
    }

    // Minimal test domain event
    private sealed class TestDomainEvent : IDomainEvent { }
}
