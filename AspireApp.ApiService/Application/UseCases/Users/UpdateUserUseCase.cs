using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class UpdateUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public UpdateUserUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> ExecuteAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
            if (user == null)
            {
                return DomainErrors.User.NotFound(userId);
            }

            // Update IsActive if provided
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    _userManager.Activate(user);
                }
                else
                {
                    _userManager.Deactivate(user);
                }
            }

            // Note: FirstName and LastName updates would require entity methods
            // For now, we'll skip them as the entity doesn't expose setters

            await _userRepository.UpdateAsync(user, ct);

            // Reload user
            var updatedUser = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
            if (updatedUser == null)
            {
                return DomainErrors.User.NotFound(userId);
            }

            var userDto = Mapper.Map<UserDto>(updatedUser);
            return Result.Success(userDto);
        }, cancellationToken);
    }
}

