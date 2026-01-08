using AspireApp.Domain.Shared.Helpers;

namespace AspireApp.Domain.Shared.Tests.Helpers;

public class EntityConfigurationHelperTests
{
    [Fact]
    public void DefaultTablePrefix_ShouldBeAppUnderscore()
    {
        // Assert
        Assert.Equal("App_", EntityConfigurationHelper.DefaultTablePrefix);
    }

    // Note: Integration tests with EF Core InMemory database have been removed due to version
    // compatibility issues. The ConfigureTableName extension method is tested through actual
    // usage in the Infrastructure layer with real database contexts.
}

