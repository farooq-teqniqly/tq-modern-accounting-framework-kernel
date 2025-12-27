namespace Teqniqly.MAF.Kernel.Domain;

/// <summary>
/// Marker interface for domain events. Domain events represent significant business occurrences
/// that have happened within an aggregate. They are raised by aggregates and can be used
/// to trigger side effects, maintain consistency, or integrate with other bounded contexts.
/// </summary>
/// <remarks>
/// All domain events should implement this interface. Domain events are immutable and should
/// contain all the information needed to understand what happened. They are collected by
/// aggregates and can be persisted and published by infrastructure layers.
/// </remarks>
public interface IDomainEvent { }
