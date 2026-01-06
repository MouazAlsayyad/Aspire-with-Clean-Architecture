using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldReturnFailureResult()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test Message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void SuccessGeneric_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = Result.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Value_OnFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Failure("Err", "Msg"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Map_ShouldTransformValue_WhenSuccess()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(20, mappedResult.Value);
    }
}
