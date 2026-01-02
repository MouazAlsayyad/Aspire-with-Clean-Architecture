using AspireApp.Twilio.Application.DTOs;
using FluentValidation;

namespace AspireApp.Twilio.Application.Validators;

public class GetMessagesRequestDtoValidator : AbstractValidator<GetMessagesRequestDto>
{
    public GetMessagesRequestDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100.");
    }
}

