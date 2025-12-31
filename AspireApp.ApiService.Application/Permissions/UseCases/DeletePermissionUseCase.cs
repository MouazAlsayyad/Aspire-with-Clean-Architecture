using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Services;

namespace AspireApp.ApiService.Application.Permissions.UseCases;

public class DeletePermissionUseCase : BaseUseCase
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionManager _permissionManager;

    public DeletePermissionUseCase(
        IPermissionRepository permissionRepository,
        IPermissionManager permissionManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _permissionRepository = permissionRepository;
        _permissionManager = permissionManager;
    }

    public async Task<Result> ExecuteAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var permission = await _permissionRepository.GetAsync(permissionId, includeDeleted: false, ct);
            if (permission == null)
            {
                return Result.Failure(DomainErrors.Permission.NotFound(permissionId));
            }

            try
            {
                await _permissionManager.ValidateDeletionAsync(permission, ct);
                await _permissionRepository.DeleteAsync(permission, ct);
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Error);
            }

            return Result.Success();
        }, cancellationToken);
    }
}

