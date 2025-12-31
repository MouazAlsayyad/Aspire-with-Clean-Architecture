using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Notifications.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;

namespace AspireApp.ApiService.Application.Notifications.UseCases;

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

                _userManager.UpdateFcmToken(user, dto.ClientFcmToken);

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

