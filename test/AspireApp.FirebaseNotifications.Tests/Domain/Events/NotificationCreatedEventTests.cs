using AspireApp.Domain.Shared.Common;
using AspireApp.FirebaseNotifications.Domain.Events;
using FluentAssertions;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Domain.Events;

public class NotificationCreatedEventTests
{
    [Fact]
    public void Constructor_ShouldCreateEvent_WithValidParameters()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.NotificationId.Should().Be(notificationId);
        domainEvent.UserId.Should().Be(userId);
    }

    [Fact]
    public void NotificationId_ShouldBeSet_FromConstructor()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.NotificationId.Should().Be(notificationId);
    }

    [Fact]
    public void UserId_ShouldBeSet_FromConstructor()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.UserId.Should().Be(userId);
    }

    [Fact]
    public void OccurredOn_ShouldBeSet_ToCurrentUtcTime()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var beforeTime = DateTime.UtcNow;

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);
        var afterTime = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeTime);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterTime);
        domainEvent.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Event_ShouldImplementIDomainEvent()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyGuids()
    {
        // Arrange
        var notificationId = Guid.Empty;
        var userId = Guid.Empty;

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.NotificationId.Should().Be(Guid.Empty);
        domainEvent.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TwoDifferentEvents_ShouldHaveDifferentOccurredOnTimes()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var event1 = new NotificationCreatedEvent(notificationId, userId);
        Thread.Sleep(1); // Ensure different timestamps
        var event2 = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        event2.OccurredOn.Should().BeOnOrAfter(event1.OccurredOn);
    }

    [Fact]
    public void Event_ShouldHaveImmutableProperties()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        // Properties should only have getters, not setters
        domainEvent.GetType().GetProperty(nameof(NotificationCreatedEvent.NotificationId))!
            .CanWrite.Should().BeFalse();
        domainEvent.GetType().GetProperty(nameof(NotificationCreatedEvent.UserId))!
            .CanWrite.Should().BeFalse();
        domainEvent.GetType().GetProperty(nameof(NotificationCreatedEvent.OccurredOn))!
            .CanWrite.Should().BeFalse();
    }

    [Fact]
    public void MultipleEvents_WithDifferentIds_ShouldBeDistinct()
    {
        // Arrange & Act
        var event1 = new NotificationCreatedEvent(Guid.NewGuid(), Guid.NewGuid());
        var event2 = new NotificationCreatedEvent(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        event1.NotificationId.Should().NotBe(event2.NotificationId);
        event1.UserId.Should().NotBe(event2.UserId);
    }

    [Fact]
    public void Event_ShouldContainAllRequiredInformation()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        // Assert
        domainEvent.NotificationId.Should().NotBeEmpty();
        domainEvent.UserId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().NotBe(default(DateTime));
    }
}

