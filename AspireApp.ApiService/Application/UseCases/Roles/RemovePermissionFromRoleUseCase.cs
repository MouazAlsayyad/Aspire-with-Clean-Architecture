using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Roles;

public class RemovePermissionFromRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;

    public RemovePermissionFromRoleUseCase(
        IRoleRepository roleRepository,
        IRoleManager roleManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task<Result> ExecuteAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var role = await _roleRepository.GetAsync(roleId, includeDeleted: false, ct);
            if (role == null)
            {
                return Result.Failure(DomainErrors.Role.NotFound(roleId));
            }

            _roleManager.RemovePermission(role, permissionId);
            await _roleRepository.UpdateAsync(role, ct);

            return Result.Success();
        }, cancellationToken);
    }
}

