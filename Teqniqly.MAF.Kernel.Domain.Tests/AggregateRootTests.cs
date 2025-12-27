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

    // Minimal test aggregate to test the abstract base class
    private sealed class TestAggregate : AggregateRoot
    {
        // Empty - just inherits from AggregateRoot
    }
}
