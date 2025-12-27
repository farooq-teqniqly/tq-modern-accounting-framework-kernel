namespace Teqniqly.MAF.Kernel.Domain;

public abstract class AggregateRoot
{
    protected AggregateRoot()
    {
        Id = Guid.CreateVersion7();
    }

    public Guid Id { get; protected set; }
}
