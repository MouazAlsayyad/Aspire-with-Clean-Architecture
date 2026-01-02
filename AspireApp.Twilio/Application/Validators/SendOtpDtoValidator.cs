using AspireApp.Twilio.Application.DTOs;
using FluentValidation;

namespace AspireApp.Twilio.Application.Validators;

public class SendOtpDtoValidator : AbstractValidator<SendOtpDto>
{
    public SendOtpDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
    }
}

