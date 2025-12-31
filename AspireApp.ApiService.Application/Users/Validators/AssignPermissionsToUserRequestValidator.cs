using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Domain.Permissions.Services;
using FluentValidation;

namespace AspireApp.ApiService.Application.Users.Validators;

public class AssignPermissionsToUserRequestValidator : AbstractValidator<AssignPermissionsToUserRequest>
{
    public AssignPermissionsToUserRequestValidator(
        IPermissionManager permissionManager)
    {
        RuleFor(x => x.PermissionIds)
            .NotNull()
            .WithMessage("Permission IDs are required.")
            .Must(permissionIds => permissionIds != null && permissionIds.Any())
            .WithMessage("At least one permission ID is required.")
            .Must(permissionIds => permissionIds == null || !permissionIds.Any() || permissionIds.All(id => id != Guid.Empty))
            .WithMessage("Permission IDs cannot contain empty GUIDs.")
            .When(x => x.PermissionIds != null && x.PermissionIds.Any())
            .MustAsync(async (permissionIds, cancellation) =>
            {
                if (permissionIds == null || !permissionIds.Any())
                    return true;

                // Check if all permission IDs exist
                foreach (var permissionId in permissionIds)
                {
                    var exists = await permissionManager.PermissionExistsAsync(permissionId, cancellation);
                    if (!exists)
                        return false;
                }
                return true;
            })
            .WithMessage("One or more permission IDs do not exist.")
            .When(x => x.PermissionIds != null && x.PermissionIds.Any());
    }
}

