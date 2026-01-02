using AspireApp.Domain.Shared.Common;
using AspireApp.ApiService.Application.Permissions.DTOs;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;

namespace AspireApp.ApiService.Application.Permissions.UseCases;

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

