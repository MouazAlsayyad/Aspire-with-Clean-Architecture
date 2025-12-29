using AspireApp.ApiService.Domain.Common;

namespace AspireApp.ApiService.Domain.Entities;

public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Creation time of this entity 
    /// </summary>
    public DateTime CreationTime { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification time of this entity 
    /// </summary>
    public DateTime? LastModificationTime { get; protected set; }

    /// <summary>
    /// ID of the user who last modified this entity
    /// </summary>
    public Guid? LastModifiedBy { get; protected set; }

    /// <summary>
    /// Deletion time of this entity
    /// </summary>
    public DateTime? DeletionTime { get; protected set; }

    /// <summary>
    /// Indicates whether this entity is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; protected set; } = false;

    /// <summary>
    /// Domain events raised by this entity
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched.
    /// Public so infrastructure layer can raise events for change tracking.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the entity as deleted (soft delete) 
    /// </summary>
    public void Delete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletionTime = DateTime.UtcNow;
        SetLastModificationTime(deletedBy: deletedBy);
    }

    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    public void Restore(Guid? restoredBy = null)
    {
        IsDeleted = false;
        DeletionTime = null;
        SetLastModificationTime(modifiedBy: restoredBy);
    }

    /// <summary>
    /// Updates the last modification time and user (called automatically by repository on update)
    /// </summary>
    public void SetLastModificationTime(DateTime? time = null, Guid? modifiedBy = null, Guid? deletedBy = null)
    {
        LastModificationTime = time ?? DateTime.UtcNow;
        LastModifiedBy = modifiedBy ?? deletedBy;
    }

    /// <summary>
    /// Sets the creation time (called automatically by repository on insert)
    /// </summary>
    public void SetCreationTime(DateTime? time = null)
    {
        CreationTime = time ?? DateTime.UtcNow;
    }
}

