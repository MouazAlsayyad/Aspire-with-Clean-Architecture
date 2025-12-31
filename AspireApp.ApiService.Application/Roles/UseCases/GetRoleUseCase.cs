using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Roles.Interfaces;

namespace AspireApp.ApiService.Application.Roles.UseCases;

public class GetRoleUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleUseCase(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleDto>> ExecuteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetAsync(roleId, includeDeleted: false, cancellationToken);
        if (role == null)
        {
            return DomainErrors.Role.NotFound(roleId);
        }

        var roleDto = Mapper.Map<RoleDto>(role);
        return Result.Success(roleDto);
    }
}

