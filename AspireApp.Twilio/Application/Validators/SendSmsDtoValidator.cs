using AspireApp.Twilio.Application.DTOs;
using FluentValidation;

namespace AspireApp.Twilio.Application.Validators;

public class SendSmsDtoValidator : AbstractValidator<SendSmsDto>
{
    public SendSmsDtoValidator()
    {
        RuleFor(x => x.ToPhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(1600)
            .WithMessage("Message must not exceed 1600 characters.");
    }
}

