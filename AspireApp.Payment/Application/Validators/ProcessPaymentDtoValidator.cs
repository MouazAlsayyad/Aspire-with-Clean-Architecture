using AspireApp.Payment.Application.DTOs;
using FluentValidation;

namespace AspireApp.Payment.Application.Validators;

/// <summary>
/// Validator for ProcessPaymentDto
/// </summary>
public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");
    }
}

