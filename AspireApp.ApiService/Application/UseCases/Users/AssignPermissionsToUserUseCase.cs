using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class AssignPermissionsToUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public AssignPermissionsToUserUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(Guid userId, AssignPermissionsToUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
                if (user == null)
                {
                    return Result.Failure(DomainErrors.User.NotFound(userId));
                }

                // Set (replace) all permissions for the user
                await _userManager.SetPermissionsAsync(user, request.PermissionIds, ct);
                await _userRepository.UpdateAsync(user, ct);

                return Result.Success();
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Error);
            }
        }, cancellationToken);
    }
}

