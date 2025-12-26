namespace AspireApp.ApiService.Domain.Entities;

public abstract class BaseEntity
{
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
    internal void SetLastModificationTime(DateTime? time = null, Guid? modifiedBy = null, Guid? deletedBy = null)
    {
        LastModificationTime = time ?? DateTime.UtcNow;
        LastModifiedBy = modifiedBy ?? deletedBy;
    }

    /// <summary>
    /// Sets the creation time (called automatically by repository on insert)
    /// </summary>
    internal void SetCreationTime(DateTime? time = null)
    {
        CreationTime = time ?? DateTime.UtcNow;
    }
}

