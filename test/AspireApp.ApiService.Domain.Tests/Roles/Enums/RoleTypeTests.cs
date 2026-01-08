using AspireApp.ApiService.Domain.Roles.Enums;

namespace AspireApp.ApiService.Domain.Tests.Roles.Enums;

public class RoleTypeTests
{
    [Fact]
    public void RoleType_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(1, (int)RoleType.Admin);
        Assert.Equal(2, (int)RoleType.User);
        Assert.Equal(3, (int)RoleType.Manager);
    }

    [Fact]
    public void RoleType_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedMembers = new[] { "Admin", "User", "Manager" };

        // Act
        var actualMembers = Enum.GetNames(typeof(RoleType));

        // Assert
        Assert.Equal(expectedMembers.Length, actualMembers.Length);
        foreach (var expected in expectedMembers)
        {
            Assert.Contains(expected, actualMembers);
        }
    }

    [Fact]
    public void RoleType_DefaultValue_ShouldBeZero()
    {
        // Arrange
        RoleType defaultValue = default;

        // Assert - default(enum) is 0, which doesn't match any defined value
        Assert.Equal(0, (int)defaultValue);
    }

    [Fact]
    public void RoleType_CanBeCompared()
    {
        // Assert
        Assert.True(RoleType.Manager > RoleType.User);
        Assert.True(RoleType.User > RoleType.Admin);
    }

    [Fact]
    public void RoleType_CanBeParsedFromString()
    {
        // Act
        var parsed = Enum.Parse<RoleType>("Admin");

        // Assert
        Assert.Equal(RoleType.Admin, parsed);
    }

    [Fact]
    public void RoleType_CanBeConvertedToString()
    {
        // Act
        var userString = RoleType.User.ToString();
        var adminString = RoleType.Admin.ToString();

        // Assert
        Assert.Equal("User", userString);
        Assert.Equal("Admin", adminString);
    }
}

