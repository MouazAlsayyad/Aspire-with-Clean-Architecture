using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Roles;

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
            Role role;
            try
            {
                // Use domain manager to create role (handles domain validation)
                role = await _roleManager.CreateAsync(
                    request.Name,
                    request.Description,
                    request.Type,
                    ct);
            }
            catch (InvalidOperationException ex)
            {
                // Convert domain exception to Result
                if (ex.Message.Contains("already exists"))
                {
                    return Result.Failure<RoleDto>(DomainErrors.Role.NameAlreadyExists(request.Name));
                }
                return Result.Failure<RoleDto>(DomainErrors.General.ServerError(ex.Message));
            }

            // Add permissions if provided
            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
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

            // Save role
            var savedRole = await _roleRepository.InsertAsync(role, ct);

            // Reload with permissions
            var roleWithPermissions = await _roleRepository.GetAsync(savedRole.Id, includeDeleted: false, ct);

            if (roleWithPermissions == null)
            {
                return Result.Failure<RoleDto>(DomainErrors.Role.NotFound(savedRole.Id));
            }

            // Map to DTO
            var roleDto = Mapper.Map<RoleDto>(roleWithPermissions);

            return Result.Success(roleDto);
        }, cancellationToken);
    }
}
