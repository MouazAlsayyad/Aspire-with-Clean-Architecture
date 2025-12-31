using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Permissions;

public class GetAllPermissionsUseCase : BaseUseCase
{
    private readonly IPermissionRepository _permissionRepository;

    public GetAllPermissionsUseCase(
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetListAsync(includeDeleted: false, cancellationToken);
        var permissionDtos = Mapper.Map<IEnumerable<PermissionDto>>(permissions);
        return Result.Success(permissionDtos);
    }
}

