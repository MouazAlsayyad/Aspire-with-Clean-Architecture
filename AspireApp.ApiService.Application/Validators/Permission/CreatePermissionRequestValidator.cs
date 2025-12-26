using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Domain.Services;
using FluentValidation;

namespace AspireApp.ApiService.Application.Validators.Permission;

public class CreatePermissionRequestValidator : AbstractValidator<CreatePermissionRequest>
{
    public CreatePermissionRequestValidator(IPermissionManager permissionManager)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Permission name is required.")
            .MaximumLength(200)
            .WithMessage("Permission name must not exceed 200 characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Permission name can only contain letters, numbers, dots, hyphens, and underscores.")
            .MustAsync(async (name, cancellation) => !await permissionManager.PermissionNameExistsAsync(name, cancellation))
            .WithMessage("Permission name already exists.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Resource)
            .NotEmpty()
            .WithMessage("Resource is required.")
            .MaximumLength(100)
            .WithMessage("Resource must not exceed 100 characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Resource can only contain letters, numbers, dots, hyphens, and underscores.");

        RuleFor(x => x.Action)
            .NotEmpty()
            .WithMessage("Action is required.")
            .MaximumLength(50)
            .WithMessage("Action must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Action can only contain letters, numbers, dots, hyphens, and underscores.");
    }
}

