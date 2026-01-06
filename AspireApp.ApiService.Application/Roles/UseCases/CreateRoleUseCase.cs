using AspireApp.Domain.Shared.Common;
using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Roles.Services;

namespace AspireApp.ApiService.Application.Roles.UseCases;

public class CreateRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;

    public CreateRoleUseCase(
        IRoleRepository roleRepository,
        IRoleManager roleManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task<Result<RoleDto>> ExecuteAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Use domain manager to create role
                // Note: Role name uniqueness and permission existence validation is handled by FluentValidation
                var role = await _roleManager.CreateAsync(
                    request.Name,
                    request.Description,
                    request.Type,
                    ct);

                // Add permissions if provided
                // Note: Permission existence validation is handled by FluentValidation, so all IDs should be valid here
                if (request.PermissionIds != null && request.PermissionIds.Any())
                {
                    foreach (var permissionId in request.PermissionIds)
                    {
                        // This should not throw NotFound since FluentValidation validates permission existence
                        await _roleManager.AddPermissionAsync(role, permissionId, ct);
                    }
                }

                // Save role
                var savedRole = await _roleRepository.InsertAsync(role, ct);
                if (savedRole == null)
                {
                    return Result.Failure<RoleDto>(DomainErrors.Role.CreationFailed);
                }

                // Reload with permissions
                var roleWithPermissions = await _roleRepository.GetAsync(savedRole.Id, includeDeleted: false, ct);

                if (roleWithPermissions == null)
                {
                    return Result.Failure<RoleDto>(DomainErrors.Role.NotFound(savedRole.Id));
                }

                // Map to DTO
                var roleDto = Mapper.Map<RoleDto>(roleWithPermissions);

                return Result.Success(roleDto);
            }
            catch (DomainException ex)
            {
                return Result.Failure<RoleDto>(ex.Error);
            }
        }, cancellationToken);
    }
}
