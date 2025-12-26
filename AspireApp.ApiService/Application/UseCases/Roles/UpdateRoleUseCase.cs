using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Roles;

public class UpdateRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;

    public UpdateRoleUseCase(
        IRoleRepository roleRepository,
        IRoleManager roleManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task<Result<RoleDto>> ExecuteAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var role = await _roleRepository.GetAsync(roleId, includeDeleted: false, ct);
            if (role == null)
            {
                return DomainErrors.Role.NotFound(roleId);
            }

            // Update description if provided
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                _roleManager.UpdateDescription(role, request.Description);
            }

            // Update permissions if provided
            if (request.PermissionIds != null)
            {
                // Remove all existing permissions
                var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
                foreach (var permissionId in existingPermissionIds)
                {
                    _roleManager.RemovePermission(role, permissionId);
                }

                // Add new permissions
                foreach (var permissionId in request.PermissionIds)
                {
                    try
                    {
                        await _roleManager.AddPermissionAsync(role, permissionId, ct);
                    }
                    catch (InvalidOperationException)
                    {
                        // Permission not found - skip it
                        continue;
                    }
                }
            }

            // Save changes
            await _roleRepository.UpdateAsync(role, ct);

            // Reload with permissions
            var updatedRole = await _roleRepository.GetAsync(roleId, includeDeleted: false, ct);

            if (updatedRole == null)
            {
                return DomainErrors.Role.NotFound(roleId);
            }

            // Map to DTO
            var roleDto = Mapper.Map<RoleDto>(updatedRole);

            return Result.Success(roleDto);
        }, cancellationToken);
    }
}
