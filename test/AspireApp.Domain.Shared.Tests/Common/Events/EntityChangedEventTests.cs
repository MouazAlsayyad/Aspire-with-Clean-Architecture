using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Common.Events;

namespace AspireApp.Domain.Shared.Tests.Common.Events;

public class EntityChangedEventTests
{
    [Fact]
    public void Constructor_ShouldSetEntityIdAndType()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";

        // Act
        var evt = new TestEntityChangedEvent(entityId, entityType);

        // Assert
        Assert.Equal(entityId, evt.EntityId);
        Assert.Equal(entityType, evt.EntityType);
    }

    [Fact]
    public void Constructor_ShouldSetOccurredOn()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var entityId = Guid.NewGuid();
        var entityType = "User";

        // Act
        var evt = new TestEntityChangedEvent(entityId, entityType);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(evt.OccurredOn >= before);
        Assert.True(evt.OccurredOn <= after);
    }

    [Fact]
    public void Constructor_WithNullEntityType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestEntityChangedEvent(entityId, null!));
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldAccept()
    {
        // Arrange
        var entityId = Guid.Empty;
        var entityType = "User";

        // Act
        var evt = new TestEntityChangedEvent(entityId, entityType);

        // Assert
        Assert.Equal(Guid.Empty, evt.EntityId);
    }

    [Fact]
    public void EntityChangedEvent_ShouldImplementIDomainEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";

        // Act
        var evt = new TestEntityChangedEvent(entityId, entityType);

        // Assert
        Assert.IsAssignableFrom<IDomainEvent>(evt);
    }

    // Test implementation class
    private class TestEntityChangedEvent : EntityChangedEvent
    {
        public TestEntityChangedEvent(Guid entityId, string entityType)
            : base(entityId, entityType)
        {
        }
    }
}

