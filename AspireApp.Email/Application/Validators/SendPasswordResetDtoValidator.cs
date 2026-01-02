using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendPasswordResetDtoValidator : AbstractValidator<SendPasswordResetDto>
{
    public SendPasswordResetDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.ResetUrl)
            .NotEmpty().WithMessage("Reset URL is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid reset URL.");
    }
}

