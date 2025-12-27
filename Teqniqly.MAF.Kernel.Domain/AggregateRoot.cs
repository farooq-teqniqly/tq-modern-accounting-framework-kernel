namespace Teqniqly.MAF.Kernel.Domain;

/// <summary>
/// Base class for aggregate roots in the domain model. An aggregate root is the entry point
/// to an aggregate and is responsible for maintaining consistency boundaries and enforcing
/// business invariants.
/// </summary>
/// <remarks>
/// <para>
/// All aggregate roots in the Accounting Kernel inherit from this class. Key features:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Automatic ID Generation</strong>: When created via the parameterless constructor,
/// a Version 7 UUID is automatically generated using <see cref="Guid.CreateVersion7()"/>.
/// Version 7 GUIDs are time-ordered, which improves database indexing performance.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Persistence Support</strong>: The constructor accepting a <see cref="Guid"/>
/// is used when loading aggregates from persistence, allowing the ID to be set from
/// stored data rather than generating a new one.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Domain Events</strong>: Aggregates can raise domain events to communicate
/// significant business occurrences. Events are collected in <see cref="DomainEvents"/>
/// and can be persisted and published by infrastructure layers.
/// </description>
/// </item>
/// </list>
/// <para>
/// Usage example:
/// </para>
/// <code>
/// public class Account : AggregateRoot
/// {
///     private Account() { } // Private constructor enforces factory method usage
///
///     private Account(Guid id) : base(id) { } // For persistence loading
///
///     public static Account Create(AccountName name)
///     {
///         var account = new Account(); // ID generated automatically
///         // ... set properties and enforce invariants
///         account.AddDomainEvent(new AccountCreated(account.Id, name));
///         return account;
///     }
/// }
/// </code>
/// </remarks>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// Automatically generates a Version 7 UUID for the <see cref="Id"/> property.
    /// </summary>
    /// <remarks>
    /// This constructor is used when creating new aggregates. The ID is generated using
    /// <see cref="Guid.CreateVersion7()"/>, which produces time-ordered GUIDs that are
    /// better for database indexing than random GUIDs.
    /// </remarks>
    protected AggregateRoot()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class with a specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate. This is used when loading
    /// aggregates from persistence, where the ID is already known.</param>
    /// <remarks>
    /// This constructor is used by infrastructure layers when reconstructing aggregates
    /// from persisted data (e.g., event sourcing, ORM materialization). Derived classes
    /// should expose this via a private constructor to maintain encapsulation.
    /// </remarks>
    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the collection of domain events that have been raised by this aggregate.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="IDomainEvent"/> instances. The collection
    /// is empty when the aggregate is first created and grows as domain events are added
    /// via <see cref="AddDomainEvent(IDomainEvent)"/>.
    /// </value>
    /// <remarks>
    /// Domain events are typically cleared after they have been persisted and published
    /// by infrastructure layers using <see cref="ClearDomainEvents()"/>.
    /// </remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets the unique identifier for this aggregate.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies this aggregate instance. The ID is
    /// automatically generated when using the parameterless constructor, or set explicitly
    /// when using the constructor that accepts a <see cref="Guid"/>.
    /// </value>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// </summary>
    /// <remarks>
    /// This method is typically called by infrastructure layers after domain events have
    /// been persisted and published. Clearing events prevents them from being processed
    /// multiple times.
    /// </remarks>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Adds a domain event to this aggregate's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add. Must not be <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// Domain events represent significant business occurrences that have happened within
    /// the aggregate. They should be raised immediately after the state change that triggers
    /// them, typically within methods that modify the aggregate's state.
    /// </para>
    /// <para>
    /// Example:
    /// </para>
    /// <code>
    /// public void RecordTransaction(TransactionEntry entry)
    /// {
    ///     _entries.Add(entry);
    ///     AddDomainEvent(new TransactionPosted(Id, entry));
    /// }
    /// </code>
    /// </remarks>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
