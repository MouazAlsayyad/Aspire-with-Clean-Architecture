using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Permissions;

public class CreatePermissionUseCase : BaseUseCase
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionManager _permissionManager;

    public CreatePermissionUseCase(
        IPermissionRepository permissionRepository,
        IPermissionManager permissionManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _permissionRepository = permissionRepository;
        _permissionManager = permissionManager;
    }

    public async Task<Result<PermissionDto>> ExecuteAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var permission = await _permissionManager.CreateAsync(
                    request.Name,
                    request.Description,
                    request.Resource,
                    request.Action,
                    ct);

                // Save permission
                var savedPermission = await _permissionRepository.InsertAsync(permission, ct);

                // Map to DTO
                var permissionDto = Mapper.Map<PermissionDto>(savedPermission);

                return Result.Success(permissionDto);
            }
            catch (DomainException ex)
            {
                return Result.Failure<PermissionDto>(ex.Error);
            }
        }, cancellationToken);
    }
}
