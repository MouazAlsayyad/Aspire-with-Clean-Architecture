using AspireApp.Payment.Application.DTOs;
using FluentValidation;

namespace AspireApp.Payment.Application.Validators;

/// <summary>
/// Validator for RefundPaymentDto
/// </summary>
public class RefundPaymentDtoValidator : AbstractValidator<RefundPaymentDto>
{
    public RefundPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Refund amount must be greater than zero");
    }
}

