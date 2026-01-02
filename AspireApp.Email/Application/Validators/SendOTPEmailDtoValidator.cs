using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendOTPEmailDtoValidator : AbstractValidator<SendOTPEmailDto>
{
    public SendOTPEmailDtoValidator()
    {
        RuleFor(x => x.ClubName)
            .NotEmpty().WithMessage("Club name is required.")
            .MaximumLength(200).WithMessage("Club name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(4, 6).WithMessage("OTP must be between 4 and 6 characters.");
    }
}

