using AspireApp.Domain.Shared.Common.Events;

namespace AspireApp.Domain.Shared.Tests.Common.Events;

public class EntityCreatedEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var properties = new Dictionary<string, object?>
        {
            { "Email", "test@test.com" },
            { "UserName", "testuser" }
        };

        // Act
        var evt = new EntityCreatedEvent(entityId, entityType, properties);

        // Assert
        Assert.Equal(entityId, evt.EntityId);
        Assert.Equal(entityType, evt.EntityType);
        Assert.Equal(properties, evt.Properties);
    }

    [Fact]
    public void Constructor_WithNullProperties_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EntityCreatedEvent(entityId, entityType, null!));
    }

    [Fact]
    public void Constructor_WithEmptyProperties_ShouldAccept()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var properties = new Dictionary<string, object?>();

        // Act
        var evt = new EntityCreatedEvent(entityId, entityType, properties);

        // Assert
        Assert.NotNull(evt.Properties);
        Assert.Empty(evt.Properties);
    }

    [Fact]
    public void Constructor_WithNullPropertyValues_ShouldAccept()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var properties = new Dictionary<string, object?>
        {
            { "Email", null },
            { "UserName", "testuser" }
        };

        // Act
        var evt = new EntityCreatedEvent(entityId, entityType, properties);

        // Assert
        Assert.Equal(2, evt.Properties.Count);
        Assert.Null(evt.Properties["Email"]);
        Assert.Equal("testuser", evt.Properties["UserName"]);
    }

    [Fact]
    public void EntityCreatedEvent_ShouldInheritFromEntityChangedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var properties = new Dictionary<string, object?>();

        // Act
        var evt = new EntityCreatedEvent(entityId, entityType, properties);

        // Assert
        Assert.IsAssignableFrom<EntityChangedEvent>(evt);
    }

    [Fact]
    public void EntityCreatedEvent_ShouldHaveOccurredOn()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var properties = new Dictionary<string, object?>();

        // Act
        var evt = new EntityCreatedEvent(entityId, entityType, properties);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(evt.OccurredOn >= before);
        Assert.True(evt.OccurredOn <= after);
    }
}

