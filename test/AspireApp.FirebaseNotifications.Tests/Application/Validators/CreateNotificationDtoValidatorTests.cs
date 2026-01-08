
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.Validators;
using AspireApp.FirebaseNotifications.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.Validators;

public class CreateNotificationDtoValidatorTests
{
    private readonly CreateNotificationDtoValidator _validator;

    public CreateNotificationDtoValidatorTests()
    {
        _validator = new CreateNotificationDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var model = CreateValidModel() with { Title = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_Length()
    {
        var model = CreateValidModel() with { Title = new string('a', 501) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Message_Is_Empty()
    {
        var model = CreateValidModel() with { Message = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = CreateValidModel() with { UserId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = CreateValidModel();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private CreateNotificationDto CreateValidModel()
    {
        return new CreateNotificationDto(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "Title Ar",
            "Message",
            "Message Ar",
            Guid.NewGuid(),
            "http://example.com"
        );
    }
}
