using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Domain.ActivityLogs.Enums;

namespace AspireApp.ApiService.Domain.Tests.ActivityLogs.Entities;

public class ActivityLogTests
{
    [Fact]
    public void Constructor_WithRequiredParameters_ShouldCreateActivityLog()
    {
        // Arrange
        var activityType = "UserCreated";
        var descriptionTemplate = "User {0} was created";

        // Act
        var activityLog = new ActivityLog(activityType, descriptionTemplate);

        // Assert
        Assert.Equal(activityType, activityLog.ActivityType);
        Assert.Equal(descriptionTemplate, activityLog.DescriptionTemplate);
        Assert.Equal(ActivitySeverity.Info, activityLog.Severity);
        Assert.True(activityLog.IsPublic);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var activityType = "UserUpdated";
        var descriptionTemplate = "User {0} updated field {1}";
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var descriptionParameters = "{\"field\":\"email\"}";
        var metadata = "{\"oldValue\":\"old@test.com\"}";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var severity = ActivitySeverity.High;
        var isPublic = false;
        var tags = "user,update,email";

        // Act
        var activityLog = new ActivityLog(
            activityType,
            descriptionTemplate,
            userId,
            userName,
            entityId,
            entityType,
            descriptionParameters,
            metadata,
            ipAddress,
            userAgent,
            severity,
            isPublic,
            tags);

        // Assert
        Assert.Equal(activityType, activityLog.ActivityType);
        Assert.Equal(descriptionTemplate, activityLog.DescriptionTemplate);
        Assert.Equal(userId, activityLog.UserId);
        Assert.Equal(userName, activityLog.UserName);
        Assert.Equal(entityId, activityLog.EntityId);
        Assert.Equal(entityType, activityLog.EntityType);
        Assert.Equal(descriptionParameters, activityLog.DescriptionParameters);
        Assert.Equal(metadata, activityLog.Metadata);
        Assert.Equal(ipAddress, activityLog.IpAddress);
        Assert.Equal(userAgent, activityLog.UserAgent);
        Assert.Equal(severity, activityLog.Severity);
        Assert.Equal(isPublic, activityLog.IsPublic);
        Assert.Equal(tags, activityLog.Tags);
    }

    [Fact]
    public void Constructor_WithNullActivityType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var descriptionTemplate = "Test description";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ActivityLog(null!, descriptionTemplate));
    }

    [Fact]
    public void Constructor_WithNullDescriptionTemplate_ShouldThrowArgumentNullException()
    {
        // Arrange
        var activityType = "TestActivity";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ActivityLog(activityType, null!));
    }

    [Fact]
    public void Constructor_WithOptionalNullParameters_ShouldAcceptNulls()
    {
        // Arrange
        var activityType = "TestActivity";
        var descriptionTemplate = "Test description";

        // Act
        var activityLog = new ActivityLog(
            activityType,
            descriptionTemplate,
            userId: null,
            userName: null,
            entityId: null,
            entityType: null,
            descriptionParameters: null,
            metadata: null,
            ipAddress: null,
            userAgent: null);

        // Assert
        Assert.Null(activityLog.UserId);
        Assert.Null(activityLog.UserName);
        Assert.Null(activityLog.EntityId);
        Assert.Null(activityLog.EntityType);
        Assert.Null(activityLog.DescriptionParameters);
        Assert.Null(activityLog.Metadata);
        Assert.Null(activityLog.IpAddress);
        Assert.Null(activityLog.UserAgent);
        Assert.Null(activityLog.Tags);
    }

    [Fact]
    public void Constructor_WithDifferentSeverityLevels_ShouldSetCorrectly()
    {
        // Arrange & Act
        var infoLog = new ActivityLog("Test", "Test", severity: ActivitySeverity.Info);
        var lowLog = new ActivityLog("Test", "Test", severity: ActivitySeverity.Low);
        var mediumLog = new ActivityLog("Test", "Test", severity: ActivitySeverity.Medium);
        var highLog = new ActivityLog("Test", "Test", severity: ActivitySeverity.High);
        var criticalLog = new ActivityLog("Test", "Test", severity: ActivitySeverity.Critical);

        // Assert
        Assert.Equal(ActivitySeverity.Info, infoLog.Severity);
        Assert.Equal(ActivitySeverity.Low, lowLog.Severity);
        Assert.Equal(ActivitySeverity.Medium, mediumLog.Severity);
        Assert.Equal(ActivitySeverity.High, highLog.Severity);
        Assert.Equal(ActivitySeverity.Critical, criticalLog.Severity);
    }

    [Fact]
    public void Constructor_WithIsPublicFalse_ShouldSetIsPublicCorrectly()
    {
        // Arrange
        var activityType = "SensitiveActivity";
        var descriptionTemplate = "Sensitive operation";

        // Act
        var activityLog = new ActivityLog(activityType, descriptionTemplate, isPublic: false);

        // Assert
        Assert.False(activityLog.IsPublic);
    }

    [Fact]
    public void ActivityLog_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var activityLog = new ActivityLog("Test", "Test");

        // Assert
        Assert.NotEqual(Guid.Empty, activityLog.Id);
        Assert.True(activityLog.CreationTime <= DateTime.UtcNow);
    }
}

