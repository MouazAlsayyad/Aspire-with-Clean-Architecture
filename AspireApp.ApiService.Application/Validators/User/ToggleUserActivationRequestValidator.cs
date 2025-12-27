using AspireApp.ApiService.Application.DTOs.User;
using FluentValidation;

namespace AspireApp.ApiService.Application.Validators.User;

/// <summary>
/// Validator for ToggleUserActivationRequest.
/// Note: IsActive is a boolean type, so no validation rules are needed.
/// This validator exists for consistency and to ensure the ValidationFilter works correctly.
/// </summary>
public class ToggleUserActivationRequestValidator : AbstractValidator<ToggleUserActivationRequest>
{
}

