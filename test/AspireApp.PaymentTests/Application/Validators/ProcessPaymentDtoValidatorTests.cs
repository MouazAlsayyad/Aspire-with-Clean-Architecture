using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.Validators;
using FluentValidation.TestHelper;

namespace AspireApp.PaymentTests.Application.Validators;

public class ProcessPaymentDtoValidatorTests
{
    private readonly ProcessPaymentDtoValidator _validator;

    public ProcessPaymentDtoValidatorTests()
    {
        _validator = new ProcessPaymentDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PaymentId_Is_Empty()
    {
        var model = new ProcessPaymentDto { PaymentId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PaymentId_Is_Valid()
    {
        var model = new ProcessPaymentDto { PaymentId = Guid.NewGuid() };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentId);
    }
}
