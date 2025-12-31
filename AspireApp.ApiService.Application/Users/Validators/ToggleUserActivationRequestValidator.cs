using AspireApp.ApiService.Application.Roles.DTOs;
using FluentValidation;

namespace AspireApp.ApiService.Application.Users.Validators;

/// <summary>
/// Validator for ToggleUserActivationRequest.
/// Note: IsActive is a boolean type, so no validation rules are needed.
/// This validator exists for consistency and to ensure the ValidationFilter works correctly.
/// </summary>
public class ToggleUserActivationRequestValidator : AbstractValidator<AssignPermissionsToUserRequest>
{
}

