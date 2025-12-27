namespace Teqniqly.MAF.Kernel.Domain;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
        Id = Guid.CreateVersion7();
    }

    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public Guid Id { get; protected set; }

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
