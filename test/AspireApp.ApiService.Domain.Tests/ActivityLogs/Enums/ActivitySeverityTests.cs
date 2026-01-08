using AspireApp.ApiService.Domain.ActivityLogs.Enums;

namespace AspireApp.ApiService.Domain.Tests.ActivityLogs.Enums;

public class ActivitySeverityTests
{
    [Fact]
    public void ActivitySeverity_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)ActivitySeverity.Info);
        Assert.Equal(1, (int)ActivitySeverity.Low);
        Assert.Equal(2, (int)ActivitySeverity.Medium);
        Assert.Equal(3, (int)ActivitySeverity.High);
        Assert.Equal(4, (int)ActivitySeverity.Critical);
    }

    [Fact]
    public void ActivitySeverity_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedMembers = new[] { "Info", "Low", "Medium", "High", "Critical" };

        // Act
        var actualMembers = Enum.GetNames(typeof(ActivitySeverity));

        // Assert
        Assert.Equal(expectedMembers.Length, actualMembers.Length);
        foreach (var expected in expectedMembers)
        {
            Assert.Contains(expected, actualMembers);
        }
    }

    [Fact]
    public void ActivitySeverity_DefaultValue_ShouldBeInfo()
    {
        // Arrange
        ActivitySeverity defaultValue = default;

        // Assert
        Assert.Equal(ActivitySeverity.Info, defaultValue);
    }

    [Fact]
    public void ActivitySeverity_CanBeCompared()
    {
        // Assert
        Assert.True(ActivitySeverity.Critical > ActivitySeverity.High);
        Assert.True(ActivitySeverity.High > ActivitySeverity.Medium);
        Assert.True(ActivitySeverity.Medium > ActivitySeverity.Low);
        Assert.True(ActivitySeverity.Low > ActivitySeverity.Info);
    }

    [Fact]
    public void ActivitySeverity_CanBeParsedFromString()
    {
        // Act
        var parsed = Enum.Parse<ActivitySeverity>("Critical");

        // Assert
        Assert.Equal(ActivitySeverity.Critical, parsed);
    }

    [Fact]
    public void ActivitySeverity_CanBeConvertedToString()
    {
        // Act
        var infoString = ActivitySeverity.Info.ToString();
        var criticalString = ActivitySeverity.Critical.ToString();

        // Assert
        Assert.Equal("Info", infoString);
        Assert.Equal("Critical", criticalString);
    }
}

