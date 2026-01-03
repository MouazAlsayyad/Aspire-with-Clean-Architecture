using AspireApp.Notifications.Application.DTOs;
using FluentValidation;

namespace AspireApp.Notifications.Application.Validators;

/// <summary>
/// Validator for SendNotificationDto
/// </summary>
public class SendNotificationDtoValidator : AbstractValidator<SendNotificationDto>
{
    public SendNotificationDtoValidator()
    {
        RuleFor(x => x.Recipient)
            .NotEmpty()
            .WithMessage("Recipient is required")
            .MaximumLength(500)
            .WithMessage("Recipient must not exceed 500 characters");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required")
            .MaximumLength(500)
            .WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Body is required")
            .MaximumLength(10000)
            .WithMessage("Body must not exceed 10000 characters");

        RuleFor(x => x.Channels)
            .NotNull()
            .WithMessage("Channels are required")
            .Must(channels => channels != null && channels.Length > 0)
            .WithMessage("At least one notification channel must be specified");
    }
}

