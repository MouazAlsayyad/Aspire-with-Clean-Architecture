using AspireApp.ApiService.Infrastructure.Helpers;
using AspireApp.Domain.Shared.Common.Events;
using AspireApp.Domain.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Helpers;

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

public class EntityChangeTrackerTests
{
    private TestDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private void SetId(BaseEntity entity, Guid id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(entity, id);
        }
    }

    [Fact]
    public void RaiseDomainEventsForChanges_ShouldAddEntityCreatedEvent_WhenEntityAdded()
    {
        // Arrange
        using var context = GetContext();
        var entity = new TestEntity { Name = "Added" };
        SetId(entity, Guid.NewGuid());
        context.TestEntities.Add(entity);

        // Act
        EntityChangeTracker.RaiseDomainEventsForChanges(context.ChangeTracker);

        // Assert
        var events = entity.DomainEvents;
        events.Should().ContainSingle();
        events.First().Should().BeOfType<EntityCreatedEvent>();
    }

    [Fact]
    public void RaiseDomainEventsForChanges_ShouldAddEntityUpdatedEvent_WhenEntityModified()
    {
        // Arrange
        using var context = GetContext();
        var entity = new TestEntity { Name = "Original" };
        SetId(entity, Guid.NewGuid());
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.Name = "Modified";

        // Act
        context.Entry(entity).State.Should().Be(EntityState.Modified);
        EntityChangeTracker.RaiseDomainEventsForChanges(context.ChangeTracker);

        // Assert
        var events = entity.DomainEvents;
        events.Should().Contain(e => e is EntityUpdatedEvent);
    }
}
