using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;

namespace AspireApp.ApiService.Application.Users.UseCases;

public class AssignRoleToUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public AssignRoleToUserUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(Guid userId, AssignRoleToUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Load the User aggregate root with UserRoles included for proper change tracking
                var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
                if (user == null)
                {
                    return Result.Failure(DomainErrors.User.NotFound(userId));
                }

                // Use domain service to set (replace) all roles for the user
                // This modifies the UserRoles collection and calls SetLastModificationTime()
                await _userManager.SetRolesAsync(user, request.RoleIds, ct);
                
                // No need to call UpdateAsync - EF Core automatically tracks changes to tracked entities
                // The SetRoles method already calls SetLastModificationTime() internally

                return Result.Success();
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Error);
            }
        }, cancellationToken);
    }
}
