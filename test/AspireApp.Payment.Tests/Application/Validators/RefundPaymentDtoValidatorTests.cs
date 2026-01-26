using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.Validators;
using FluentValidation.TestHelper;

namespace AspireApp.Payment.Tests.Application.Validators;

public class RefundPaymentDtoValidatorTests
{
    private readonly RefundPaymentDtoValidator _validator;

    public RefundPaymentDtoValidatorTests()
    {
        _validator = new RefundPaymentDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PaymentId_Is_Empty()
    {
        var model = new RefundPaymentDto { PaymentId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative()
    {
        var model = new RefundPaymentDto { Amount = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);

        model = new RefundPaymentDto { Amount = -1 };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new RefundPaymentDto
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentId);
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
