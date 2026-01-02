using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendMembershipEmailDtoValidator : AbstractValidator<SendMembershipEmailDto>
{
    public SendMembershipEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.PlayerName)
            .NotEmpty().WithMessage("Player name is required.")
            .MaximumLength(100).WithMessage("Player name cannot exceed 100 characters.");

        RuleFor(x => x.MembershipDate)
            .NotEmpty().WithMessage("Membership date is required.");

        RuleFor(x => x.PaymentLink)
            .NotEmpty().WithMessage("Payment link is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid payment link URL.");

        RuleFor(x => x.TenantEmail)
            .EmailAddress().WithMessage("Invalid tenant email address.")
            .When(x => !string.IsNullOrEmpty(x.TenantEmail));
    }
}

