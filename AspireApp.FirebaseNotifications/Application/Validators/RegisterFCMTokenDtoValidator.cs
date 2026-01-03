using AspireApp.FirebaseNotifications.Application.DTOs;
using FluentValidation;

namespace AspireApp.FirebaseNotifications.Application.Validators;

public class RegisterFCMTokenDtoValidator : AbstractValidator<RegisterFCMTokenDto>
{
    public RegisterFCMTokenDtoValidator()
    {
        RuleFor(x => x.ClientFcmToken)
            .NotEmpty()
            .WithMessage("FCM token is required.")
            .MaximumLength(500)
            .WithMessage("FCM token must not exceed 500 characters.");
    }
}

