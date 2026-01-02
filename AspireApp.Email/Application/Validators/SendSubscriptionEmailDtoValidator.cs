using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendSubscriptionEmailDtoValidator : AbstractValidator<SendSubscriptionEmailDto>
{
    public SendSubscriptionEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");

        RuleFor(x => x.SubscriptionType)
            .NotEmpty().WithMessage("Subscription type is required.")
            .MaximumLength(100).WithMessage("Subscription type cannot exceed 100 characters.");

        RuleFor(x => x.Length)
            .NotEmpty().WithMessage("Subscription length is required.");
    }
}

