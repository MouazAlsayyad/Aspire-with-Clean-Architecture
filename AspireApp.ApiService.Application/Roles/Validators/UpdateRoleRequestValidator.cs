using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Domain.Roles.Services;
using FluentValidation;

namespace AspireApp.ApiService.Application.Roles.Validators;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator(IRoleManager roleManager)
    {
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description != null);

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

