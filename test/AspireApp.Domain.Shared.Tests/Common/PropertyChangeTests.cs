using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class PropertyChangeTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateEmptyPropertyChange()
    {
        // Act
        var propertyChange = new PropertyChange();

        // Assert
        Assert.Null(propertyChange.OldValue);
        Assert.Null(propertyChange.NewValue);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetProperties()
    {
        // Arrange
        var oldValue = "Old Value";
        var newValue = "New Value";

        // Act
        var propertyChange = new PropertyChange(oldValue, newValue);

        // Assert
        Assert.Equal(oldValue, propertyChange.OldValue);
        Assert.Equal(newValue, propertyChange.NewValue);
    }

    [Fact]
    public void Constructor_WithNullValues_ShouldAcceptNulls()
    {
        // Act
        var propertyChange = new PropertyChange(null, null);

        // Assert
        Assert.Null(propertyChange.OldValue);
        Assert.Null(propertyChange.NewValue);
    }

    [Fact]
    public void Constructor_WithMixedNullValues_ShouldWork()
    {
        // Act
        var propertyChange1 = new PropertyChange("Old", null);
        var propertyChange2 = new PropertyChange(null, "New");

        // Assert
        Assert.Equal("Old", propertyChange1.OldValue);
        Assert.Null(propertyChange1.NewValue);
        Assert.Null(propertyChange2.OldValue);
        Assert.Equal("New", propertyChange2.NewValue);
    }

    [Fact]
    public void PropertyChange_WithDifferentTypes_ShouldAcceptObjects()
    {
        // Arrange
        var oldValue = 10;
        var newValue = "20";

        // Act
        var propertyChange = new PropertyChange(oldValue, newValue);

        // Assert
        Assert.Equal(10, propertyChange.OldValue);
        Assert.Equal("20", propertyChange.NewValue);
    }

    [Fact]
    public void PropertyChange_WithComplexObjects_ShouldStore()
    {
        // Arrange
        var oldValue = new { Id = 1, Name = "Old" };
        var newValue = new { Id = 1, Name = "New" };

        // Act
        var propertyChange = new PropertyChange(oldValue, newValue);

        // Assert
        Assert.NotNull(propertyChange.OldValue);
        Assert.NotNull(propertyChange.NewValue);
        Assert.NotEqual(propertyChange.OldValue, propertyChange.NewValue);
    }

    [Fact]
    public void PropertyChange_SettersWork()
    {
        // Arrange
        var propertyChange = new PropertyChange();

        // Act
        propertyChange.OldValue = "Old";
        propertyChange.NewValue = "New";

        // Assert
        Assert.Equal("Old", propertyChange.OldValue);
        Assert.Equal("New", propertyChange.NewValue);
    }
}

