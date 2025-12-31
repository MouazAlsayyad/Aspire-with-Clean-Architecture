using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Roles.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Roles;

public class GetAllRolesUseCase : BaseUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesUseCase(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetListAsync(includeDeleted: false, cancellationToken);
        var roleDtos = Mapper.Map<IEnumerable<RoleDto>>(roles);
        return Result.Success(roleDtos);
    }
}

