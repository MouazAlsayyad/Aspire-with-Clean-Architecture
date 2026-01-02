using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendPayoutRejectionDtoValidator : AbstractValidator<SendPayoutRejectionDto>
{
    public SendPayoutRejectionDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");
    }
}

