using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Authentication.Validators;
using AspireApp.ApiService.Domain.Users.Services;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Authentication.Validators;

public class RegisterRequestValidatorTests
{
    private readonly IUserManager _userManager;
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _userManager = Substitute.For<IUserManager>();
        _validator = new RegisterRequestValidator(_userManager);
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123", "First", "Last");
        _userManager.EmailExistsAsync(request.Email, Arg.Any<CancellationToken>()).Returns(false);
        _userManager.UserNameExistsAsync(request.UserName, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("", "testuser", "Password123", "First", "Last");

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
        var request = new RegisterRequest("invalid-email", "testuser", "Password123", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public async Task Validate_WithExistingEmail_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("existing@test.com", "testuser", "Password123", "First", "Last");
        _userManager.EmailExistsAsync(request.Email, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage.Contains("already registered"));
    }

    [Fact]
    public async Task Validate_WithShortUserName_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "ab", "Password123", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName" && e.ErrorMessage.Contains("3 characters"));
    }

    [Fact]
    public async Task Validate_WithInvalidUserNameCharacters_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "test-user!", "Password123", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName" && e.ErrorMessage.Contains("letters, numbers, and underscores"));
    }

    [Fact]
    public async Task Validate_WithExistingUserName_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "existinguser", "Password123", "First", "Last");
        _userManager.EmailExistsAsync(request.Email, Arg.Any<CancellationToken>()).Returns(false);
        _userManager.UserNameExistsAsync(request.UserName, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName" && e.ErrorMessage.Contains("already taken"));
    }

    [Fact]
    public async Task Validate_WithShortPassword_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Pass1", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingUppercase_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "password123", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "PASSWORD123", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingDigit_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password", "First", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage.Contains("digit"));
    }

    [Fact]
    public async Task Validate_WithEmptyFirstName_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123", "", "Last");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Validate_WithEmptyLastName_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123", "First", "");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName");
    }
}

