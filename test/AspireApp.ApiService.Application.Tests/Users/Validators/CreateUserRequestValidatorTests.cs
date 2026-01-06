using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.Validators;
using AspireApp.ApiService.Domain.Users.Services;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Users.Validators;

public class CreateUserRequestValidatorTests
{
    private readonly IUserManager _userManager;
    private readonly CreateUserRequestValidator _validator;

    public CreateUserRequestValidatorTests()
    {
        _userManager = Substitute.For<IUserManager>();
        _validator = new CreateUserRequestValidator(_userManager);
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "testuser", "Password123!", "First", "Last");
        _userManager.EmailExistsAsync(request.Email).Returns(false);
        _userManager.UserNameExistsAsync(request.UserName).Returns(false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithExistingEmail_ShouldHaveError()
    {
        // Arrange
        var request = new CreateUserRequest("existing@example.com", "testuser", "Password123!", "First", "Last");
        _userManager.EmailExistsAsync(request.Email).Returns(true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Email) && e.ErrorMessage.Contains("already registered"));
    }

    [Theory]
    [InlineData("too")] // Too short
    [InlineData("no_digits")]
    [InlineData("NO_LOWER")]
    [InlineData("no_upper")]
    public async Task Validate_WithWeakPassword_ShouldHaveError(string password)
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "user", password, "F", "L");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Password));
    }
}
