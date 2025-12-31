using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Roles.Services;

namespace AspireApp.ApiService.Application.Roles.UseCases;

public class AddPermissionToRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;

    public AddPermissionToRoleUseCase(
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

            try
            {
                await _roleManager.AddPermissionAsync(role, permissionId, ct);
                await _roleRepository.UpdateAsync(role, ct);
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Error);
            }

            return Result.Success();
        }, cancellationToken);
    }
}

