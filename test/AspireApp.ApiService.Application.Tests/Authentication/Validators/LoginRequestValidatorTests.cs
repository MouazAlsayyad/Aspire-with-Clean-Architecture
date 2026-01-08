using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Authentication.Validators;

namespace AspireApp.ApiService.Application.Tests.Authentication.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest("", "password");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest("invalid-email", "password");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }
}

