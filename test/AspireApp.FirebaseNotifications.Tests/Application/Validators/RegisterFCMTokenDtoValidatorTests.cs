
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.Validators;

public class RegisterFCMTokenDtoValidatorTests
{
    private readonly RegisterFCMTokenDtoValidator _validator;

    public RegisterFCMTokenDtoValidatorTests()
    {
        _validator = new RegisterFCMTokenDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Token_Is_Empty()
    {
        var model = new RegisterFCMTokenDto(string.Empty);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ClientFcmToken);
    }

    [Fact]
    public void Should_Have_Error_When_Token_Exceeds_Length()
    {
        var model = new RegisterFCMTokenDto(new string('a', 501));
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ClientFcmToken);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new RegisterFCMTokenDto("valid-token");
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
