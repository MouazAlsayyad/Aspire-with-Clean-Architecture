using AspireApp.Twilio.Application.DTOs;
using FluentValidation;

namespace AspireApp.Twilio.Application.Validators;

public class ValidateOtpDtoValidator : AbstractValidator<ValidateOtpDto>
{
    public ValidateOtpDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format.");

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .WithMessage("OTP code is required.")
            .Length(4)
            .WithMessage("OTP code must be exactly 4 digits.")
            .Matches(@"^\d{4}$")
            .WithMessage("OTP code must contain only digits.");
    }
}

