using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Entities;

namespace AspireApp.Domain.Shared.Tests.Entities;

public class BaseEntityTests
{
    // Test implementation of BaseEntity
    private class TestEntity : BaseEntity { }
    private class TestEvent : IDomainEvent
    {
        public DateTime OccurredOn => DateTime.UtcNow;
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.CreationTime <= DateTime.UtcNow);
        Assert.False(entity.IsDeleted);
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = Guid.NewGuid();

        // Act
        entity.Delete(userId);

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.NotNull(entity.DeletionTime);
        Assert.Equal(userId, entity.LastModifiedBy);
    }

    [Fact]
    public void DomainEvents_Management_ShouldWork()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestEvent();

        // Act
        entity.AddDomainEvent(domainEvent);
        Assert.Single(entity.DomainEvents);

        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }
}
