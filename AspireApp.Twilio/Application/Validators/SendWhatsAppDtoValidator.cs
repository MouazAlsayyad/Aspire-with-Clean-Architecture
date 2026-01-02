using AspireApp.Twilio.Application.DTOs;
using FluentValidation;

namespace AspireApp.Twilio.Application.Validators;

public class SendWhatsAppDtoValidator : AbstractValidator<SendWhatsAppDto>
{
    public SendWhatsAppDtoValidator()
    {
        RuleFor(x => x.ToPhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required when TemplateId is not provided.")
            .MaximumLength(4096)
            .WithMessage("Message must not exceed 4096 characters.")
            .When(x => string.IsNullOrWhiteSpace(x.TemplateId));

        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required when Message is not provided.")
            .When(x => string.IsNullOrWhiteSpace(x.Message));
    }
}

