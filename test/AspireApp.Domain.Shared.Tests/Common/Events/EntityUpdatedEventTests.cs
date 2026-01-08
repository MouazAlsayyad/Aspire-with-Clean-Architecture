using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Common.Events;

namespace AspireApp.Domain.Shared.Tests.Common.Events;

public class EntityUpdatedEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var changedProperties = new Dictionary<string, PropertyChange>
        {
            { "Email", new PropertyChange("old@test.com", "new@test.com") },
            { "FirstName", new PropertyChange("Old", "New") }
        };

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);

        // Assert
        Assert.Equal(entityId, evt.EntityId);
        Assert.Equal(entityType, evt.EntityType);
        Assert.Equal(changedProperties, evt.ChangedProperties);
    }

    [Fact]
    public void Constructor_WithNullChangedProperties_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EntityUpdatedEvent(entityId, entityType, null!));
    }

    [Fact]
    public void Constructor_WithEmptyChangedProperties_ShouldAccept()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var changedProperties = new Dictionary<string, PropertyChange>();

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);

        // Assert
        Assert.NotNull(evt.ChangedProperties);
        Assert.Empty(evt.ChangedProperties);
    }

    [Fact]
    public void EntityUpdatedEvent_ShouldInheritFromEntityChangedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var changedProperties = new Dictionary<string, PropertyChange>();

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);

        // Assert
        Assert.IsAssignableFrom<EntityChangedEvent>(evt);
    }

    [Fact]
    public void EntityUpdatedEvent_ShouldHaveOccurredOn()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var changedProperties = new Dictionary<string, PropertyChange>();

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(evt.OccurredOn >= before);
        Assert.True(evt.OccurredOn <= after);
    }

    [Fact]
    public void ChangedProperties_ShouldContainOldAndNewValues()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var emailChange = new PropertyChange("old@test.com", "new@test.com");
        var isActiveChange = new PropertyChange(false, true);
        var changedProperties = new Dictionary<string, PropertyChange>
        {
            { "Email", emailChange },
            { "IsActive", isActiveChange }
        };

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);

        // Assert
        Assert.Equal(2, evt.ChangedProperties.Count);
        Assert.Equal("old@test.com", evt.ChangedProperties["Email"].OldValue);
        Assert.Equal("new@test.com", evt.ChangedProperties["Email"].NewValue);
        Assert.Equal(false, evt.ChangedProperties["IsActive"].OldValue);
        Assert.Equal(true, evt.ChangedProperties["IsActive"].NewValue);
    }

    [Fact]
    public void ChangedProperties_WithNullValues_ShouldWork()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var changedProperties = new Dictionary<string, PropertyChange>
        {
            { "MiddleName", new PropertyChange(null, "NewMiddle") },
            { "Suffix", new PropertyChange("Jr.", null) }
        };

        // Act
        var evt = new EntityUpdatedEvent(entityId, entityType, changedProperties);

        // Assert
        Assert.Null(evt.ChangedProperties["MiddleName"].OldValue);
        Assert.Equal("NewMiddle", evt.ChangedProperties["MiddleName"].NewValue);
        Assert.Equal("Jr.", evt.ChangedProperties["Suffix"].OldValue);
        Assert.Null(evt.ChangedProperties["Suffix"].NewValue);
    }
}

