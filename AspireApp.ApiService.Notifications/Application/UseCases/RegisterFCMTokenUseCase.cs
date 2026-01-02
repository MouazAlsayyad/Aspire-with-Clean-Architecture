using AspireApp.Domain.Shared.Common;
using AspireApp.ApiService.Notifications.Application.DTOs;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;

namespace AspireApp.ApiService.Notifications.Application.UseCases;

public class RegisterFCMTokenUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public RegisterFCMTokenUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result<RegisterFCMTokenResponseDto>> ExecuteAsync(
        Guid userId,
        RegisterFCMTokenDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var user = await _userRepository.GetAsync(userId, cancellationToken: ct);
                if (user == null)
                {
                    return Result.Failure<RegisterFCMTokenResponseDto>(DomainErrors.User.NotFound(userId));
                }

                // Update FCM token and register user in Firebase Auth if needed
                await _userManager.UpdateFcmTokenAsync(user, dto.ClientFcmToken, ct);

                return Result.Success(new RegisterFCMTokenResponseDto(
                    true,
                    "FCM token registered successfully"
                ));
            }
            catch (Exception ex)
            {
                return Result.Failure<RegisterFCMTokenResponseDto>(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

