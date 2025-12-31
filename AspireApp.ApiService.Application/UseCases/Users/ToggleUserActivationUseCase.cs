using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class ToggleUserActivationUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public ToggleUserActivationUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> ExecuteAsync(Guid userId, ToggleUserActivationRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
            if (user == null)
            {
                return DomainErrors.User.NotFound(userId);
            }

            try
            {
                if (request.IsActive)
                {
                    _userManager.Activate(user);
                }
                else
                {
                    _userManager.Deactivate(user);
                }

                await _userRepository.UpdateAsync(user, ct);

                // Reload user to get updated data
                var updatedUser = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
                if (updatedUser == null)
                {
                    return DomainErrors.User.NotFound(userId);
                }

                var userDto = Mapper.Map<UserDto>(updatedUser);
                return Result.Success(userDto);
            }
            catch (DomainException ex)
            {
                return Result.Failure<UserDto>(ex.Error);
            }
        }, cancellationToken);
    }
}

