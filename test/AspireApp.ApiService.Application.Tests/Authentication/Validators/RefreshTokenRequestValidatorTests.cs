using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Authentication.Validators;

namespace AspireApp.ApiService.Application.Tests.Authentication.Validators;

public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator;

    public RefreshTokenRequestValidatorTests()
    {
        _validator = new RefreshTokenRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRefreshToken_ShouldNotHaveErrors()
    {
        // Arrange
        var validToken = new string('a', 64); // 64 character token
        var request = new RefreshTokenRequest(validToken);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyRefreshToken_ShouldHaveError()
    {
        // Arrange
        var request = new RefreshTokenRequest("");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public async Task Validate_WithShortRefreshToken_ShouldHaveError()
    {
        // Arrange
        var shortToken = new string('a', 63); // Less than 64 characters
        var request = new RefreshTokenRequest(shortToken);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken" && e.ErrorMessage.Contains("64"));
    }
}

