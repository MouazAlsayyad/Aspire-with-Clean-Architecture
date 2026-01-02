namespace AspireApp.Domain.Shared.Common;

/// <summary>
/// Represents a change to a single property, containing both the old and new values.
/// Used by <see cref="Events.EntityUpdatedEvent"/> to track property changes.
/// </summary>
public class PropertyChange
{
    /// <summary>
    /// Gets or sets the previous value of the property before the change.
    /// </summary>
    public object? OldValue { get; set; }

    /// <summary>
    /// Gets or sets the new value of the property after the change.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChange"/> class.
    /// </summary>
    public PropertyChange()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChange"/> class with specified values.
    /// </summary>
    /// <param name="oldValue">The previous value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    public PropertyChange(object? oldValue, object? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

