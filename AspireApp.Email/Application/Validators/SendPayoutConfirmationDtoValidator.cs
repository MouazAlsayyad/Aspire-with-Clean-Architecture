using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendPayoutConfirmationDtoValidator : AbstractValidator<SendPayoutConfirmationDto>
{
    public SendPayoutConfirmationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}

