using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Domain.Services;
using FluentValidation;
using System.Linq;

namespace AspireApp.ApiService.Application.Validators.Role;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator(
        IRoleManager roleManager)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MinimumLength(2)
            .WithMessage("Role name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Role name must not exceed 100 characters.")
            .Matches("^[a-zA-Z0-9\\s_-]+$")
            .WithMessage("Role name can only contain letters, numbers, spaces, hyphens, and underscores.")
            .MustAsync(async (name, cancellation) => !await roleManager.RoleNameExistsAsync(name, cancellation))
            .WithMessage("Role name already exists.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Role type must be a valid enum value.");

        RuleFor(x => x.PermissionIds)
            .Must(permissionIds => permissionIds == null || !permissionIds.Any() || permissionIds.All(id => id != Guid.Empty))
            .WithMessage("Permission IDs cannot contain empty GUIDs.")
            .When(x => x.PermissionIds != null && x.PermissionIds.Any())
            .MustAsync(async (permissionIds, cancellation) =>
            {
                if (permissionIds == null || !permissionIds.Any())
                    return true;

                // Check if all permission IDs exist using role manager
                foreach (var permissionId in permissionIds)
                {
                    var exists = await roleManager.PermissionExistsAsync(permissionId, cancellation);
                    if (!exists)
                        return false;
                }
                return true;
            })
            .WithMessage("One or more permission IDs do not exist.")
            .When(x => x.PermissionIds != null && x.PermissionIds.Any());
    }
}

