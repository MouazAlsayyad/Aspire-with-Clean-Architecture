using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendOnboardingEmailDtoValidator : AbstractValidator<SendOnboardingEmailDto>
{
    public SendOnboardingEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.OnboardingUrl)
            .NotEmpty().WithMessage("Onboarding URL is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid onboarding URL.");
    }
}

