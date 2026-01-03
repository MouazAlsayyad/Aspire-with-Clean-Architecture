using AspireApp.Email.Application.DTOs;
using FluentValidation;

namespace AspireApp.Email.Application.Validators;

public class SendBookingEmailDtoValidator : AbstractValidator<SendBookingEmailDto>
{
    public SendBookingEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.PlayerName)
            .NotEmpty().WithMessage("Player name is required.")
            .MaximumLength(100).WithMessage("Player name cannot exceed 100 characters.");

        RuleFor(x => x.CourtName)
            .NotEmpty().WithMessage("Court name is required.")
            .MaximumLength(200).WithMessage("Court name cannot exceed 200 characters.");

        RuleFor(x => x.BookingDate)
            .NotEmpty().WithMessage("Booking date is required.");

        RuleFor(x => x.TimeStr)
            .NotEmpty().WithMessage("Booking time is required.");

        RuleFor(x => x.PaymentLink)
            .NotEmpty().WithMessage("Payment link is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid payment link URL.");
    }
}

