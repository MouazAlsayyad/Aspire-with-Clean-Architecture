using AspireApp.Payment.Application.DTOs;
using FluentValidation;

namespace AspireApp.Payment.Application.Validators;

/// <summary>
/// Validator for CreatePaymentDto
/// </summary>
public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Amount cannot exceed 999999.99");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code (e.g., USD, AED, SAR)");

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Invalid payment method");

        When(x => !string.IsNullOrEmpty(x.CustomerEmail), () =>
        {
            RuleFor(x => x.CustomerEmail)
                .EmailAddress()
                .WithMessage("Invalid email address");
        });

        When(x => !string.IsNullOrEmpty(x.CustomerPhone), () =>
        {
            RuleFor(x => x.CustomerPhone)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format");
        });
    }
}

