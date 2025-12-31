using AspireApp.ApiService.Application.Users.DTOs;
using FluentValidation;

namespace AspireApp.ApiService.Application.Users.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.")
            .When(x => x.LastName != null);
    }
}

