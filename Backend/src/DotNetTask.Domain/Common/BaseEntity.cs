using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetTask.Domain.Common;

/// <summary>
/// Represents the base entity class that provides
/// a unique identifier for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a read-only collection of domain events raised by the entity.
    /// These events are processed during the unit of work commit.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => this._domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity's internal event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to be recorded.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        this._domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all recorded domain events from the entity.
    /// Typically called after events have been successfully dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        this._domainEvents.Clear();
    }
}
