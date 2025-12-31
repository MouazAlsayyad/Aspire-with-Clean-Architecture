using AspireApp.ApiService.Application.Notifications.DTOs;
using FluentValidation;

namespace AspireApp.ApiService.Application.Notifications.Validators;

public class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(500)
            .WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.TitleAr)
            .MaximumLength(500)
            .WithMessage("Arabic title must not exceed 500 characters.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(2000)
            .WithMessage("Message must not exceed 2000 characters.");

        RuleFor(x => x.MessageAr)
            .MaximumLength(2000)
            .WithMessage("Arabic message must not exceed 2000 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.ActionUrl)
            .MaximumLength(500)
            .WithMessage("Action URL must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ActionUrl));
    }
}

