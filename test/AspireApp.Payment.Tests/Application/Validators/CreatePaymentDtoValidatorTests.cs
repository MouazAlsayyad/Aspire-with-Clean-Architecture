using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.Validators;
using AspireApp.Payment.Domain.Enums;
using FluentValidation.TestHelper;

namespace AspireApp.Payment.Tests.Application.Validators;

public class CreatePaymentDtoValidatorTests
{
    private readonly CreatePaymentDtoValidator _validator;

    public CreatePaymentDtoValidatorTests()
    {
        _validator = new CreatePaymentDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative()
    {
        var model = new CreatePaymentDto { Amount = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);

        model = new CreatePaymentDto { Amount = -1 };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Exceeds_Limit()
    {
        var model = new CreatePaymentDto { Amount = 1000000m };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Empty_Or_Invalid_Length()
    {
        var model = new CreatePaymentDto { Currency = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Currency);

        model = new CreatePaymentDto { Currency = "US" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
        
        model = new CreatePaymentDto { Currency = "USDD" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Have_Error_When_Method_Is_Invalid()
    {
        var model = new CreatePaymentDto { Method = (PaymentMethod)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Method);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerEmail_Is_Invalid()
    {
        var model = new CreatePaymentDto { CustomerEmail = "invalid-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerPhone_Is_Invalid()
    {
        var model = new CreatePaymentDto { CustomerPhone = "invalid-phone" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CustomerPhone);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new CreatePaymentDto
        {
            Amount = 100,
            Currency = "USD",
            Method = PaymentMethod.Stripe,
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
        result.ShouldNotHaveValidationErrorFor(x => x.Method);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerEmail);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerPhone);
    }
}
