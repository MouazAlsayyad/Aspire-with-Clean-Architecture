using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Permissions.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;

namespace AspireApp.ApiService.Application.Permissions.UseCases;

public class GetPermissionUseCase : BaseUseCase
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionUseCase(
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<PermissionDto>> ExecuteAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetAsync(permissionId, includeDeleted: false, cancellationToken);
        if (permission == null)
        {
            return DomainErrors.Permission.NotFound(permissionId);
        }

        var permissionDto = Mapper.Map<PermissionDto>(permission);
        return Result.Success(permissionDto);
    }
}

