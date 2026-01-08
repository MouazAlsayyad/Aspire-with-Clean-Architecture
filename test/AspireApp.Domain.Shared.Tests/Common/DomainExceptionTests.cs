using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithError_ShouldSetErrorAndMessage()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test error message");

        // Act
        var exception = new DomainException(error);

        // Assert
        Assert.Equal(error, exception.Error);
        Assert.Equal(error.Message, exception.Message);
    }

    [Fact]
    public void Constructor_WithErrorAndInnerException_ShouldSetAllProperties()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error message");
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new DomainException(error, innerException);

        // Assert
        Assert.Equal(error, exception.Error);
        Assert.Equal(error.Message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void DomainException_ShouldBeInstanceOfException()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error message");

        // Act
        var exception = new DomainException(error);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void DomainException_CanBeCaughtAsException()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error message");
        var caught = false;

        // Act
        try
        {
            throw new DomainException(error);
        }
        catch (Exception)
        {
            caught = true;
        }

        // Assert
        Assert.True(caught);
    }

    [Fact]
    public void DomainException_WithInnerException_ShouldPreserveStackTrace()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner");
        var error = Error.Failure("Test.Error", "Test error");

        // Act
        var exception = new DomainException(error, innerException);

        // Assert
        Assert.NotNull(exception.InnerException);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }
}

