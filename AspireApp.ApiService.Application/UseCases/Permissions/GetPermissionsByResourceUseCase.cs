using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Permissions;

public class GetPermissionsByResourceUseCase : BaseUseCase
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsByResourceUseCase(
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> ExecuteAsync(string resource, CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetByResourceAsync(resource, cancellationToken);
        var permissionDtos = Mapper.Map<IEnumerable<PermissionDto>>(permissions);
        return Result.Success(permissionDtos);
    }
}

