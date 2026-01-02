using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendCompletedBookingEmailDtoValidator : AbstractValidator<SendCompletedBookingEmailDto>
{
    public SendCompletedBookingEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.CourtName)
            .NotEmpty().WithMessage("Court name is required.");

        RuleFor(x => x.BookingDate)
            .NotEmpty().WithMessage("Booking date is required.");

        RuleFor(x => x.TimeStr)
            .NotEmpty().WithMessage("Booking time is required.");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}

