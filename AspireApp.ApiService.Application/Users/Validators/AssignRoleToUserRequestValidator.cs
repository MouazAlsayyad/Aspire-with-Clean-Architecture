using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Roles.Services;
using FluentValidation;

namespace AspireApp.ApiService.Application.Users.Validators;

public class AssignRoleToUserRequestValidator : AbstractValidator<AssignRoleToUserRequest>
{
    public AssignRoleToUserRequestValidator(
        IRoleManager roleManager)
    {
        RuleFor(x => x.RoleIds)
            .NotNull()
            .WithMessage("Role IDs are required.")
            .Must(roleIds => roleIds != null && roleIds.Any())
            .WithMessage("At least one role ID is required.")
            .Must(roleIds => roleIds == null || !roleIds.Any() || roleIds.All(id => id != Guid.Empty))
            .WithMessage("Role IDs cannot contain empty GUIDs.")
            .When(x => x.RoleIds != null && x.RoleIds.Any())
            .MustAsync(async (roleIds, cancellation) =>
            {
                if (roleIds == null || !roleIds.Any())
                    return true;

                // Check if all role IDs exist
                foreach (var roleId in roleIds)
                {
                    var exists = await roleManager.RoleExistsAsync(roleId, cancellation);
                    if (!exists)
                        return false;
                }
                return true;
            })
            .WithMessage("One or more role IDs do not exist.")
            .When(x => x.RoleIds != null && x.RoleIds.Any());
    }
}

