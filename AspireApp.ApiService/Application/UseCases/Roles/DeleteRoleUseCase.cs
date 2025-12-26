using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Roles;

public class DeleteRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;

    public DeleteRoleUseCase(
        IRoleRepository roleRepository,
        IRoleManager roleManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task<Result> ExecuteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var role = await _roleRepository.GetAsync(roleId, includeDeleted: false, ct);
            if (role == null)
            {
                return Result.Failure(DomainErrors.Role.NotFound(roleId));
            }

            _roleManager.ValidateDeletion(role);
            await _roleRepository.DeleteAsync(role, ct);

            return Result.Success();
        }, cancellationToken);
    }
}

