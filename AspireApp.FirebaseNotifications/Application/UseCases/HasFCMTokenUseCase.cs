using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;

namespace AspireApp.FirebaseNotifications.Application.UseCases;

public class HasFCMTokenUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;

    public HasFCMTokenUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetAsync(userId, cancellationToken: cancellationToken);
            if (user == null)
            {
                return Result.Failure<bool>(DomainErrors.User.NotFound(userId));
            }

            var hasToken = !string.IsNullOrWhiteSpace(user.FcmToken);
            return Result.Success(hasToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(DomainErrors.General.ServerError(ex.Message));
        }
    }
}

