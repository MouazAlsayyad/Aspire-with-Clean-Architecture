using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Create_ShouldReturnErrorWithCorrectProperties()
    {
        // Arrange
        var code = "User.NotFound";
        var message = "User not found";
        var type = ErrorType.NotFound;

        // Act
        var error = Error.Create(code, message, type);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(type, error.Type);
    }

    [Fact]
    public void Validation_Helper_ShouldSetCorrectType()
    {
        // Act
        var error = Error.Validation("Code", "Msg");

        // Assert
        Assert.Equal(ErrorType.Validation, error.Type);
    }
}
