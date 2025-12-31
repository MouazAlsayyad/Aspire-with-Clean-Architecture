using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;

namespace AspireApp.ApiService.Application.Users.UseCases;

public class RemoveRoleFromUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManager _userManager;

    public RemoveRoleFromUserUseCase(
        IUserRepository userRepository,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
            if (user == null)
            {
                return Result.Failure(DomainErrors.User.NotFound(userId));
            }

            try
            {
                _userManager.RemoveRole(user, roleId);
                await _userRepository.UpdateAsync(user, ct);
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Error);
            }

            return Result.Success();
        }, cancellationToken);
    }
}

