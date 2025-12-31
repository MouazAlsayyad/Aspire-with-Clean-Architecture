using AspireApp.ApiService.Application.Authentication.DTOs;
using FluentValidation;

namespace AspireApp.ApiService.Application.Authentication.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MinimumLength(64)
            .WithMessage("Refresh token must be at least 64 characters long.");
    }
}

