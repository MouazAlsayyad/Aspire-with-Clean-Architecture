using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Application.Users.UseCases;

public class UpdatePasswordUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserManager _userManager;

    public UpdatePasswordUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(Guid userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            var user = await _userRepository.GetAsync(userId, includeDeleted: false, ct);
            if (user == null)
            {
                return DomainErrors.User.NotFound(userId);
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash.Hash, user.PasswordHash.Salt))
            {
                return DomainErrors.User.InvalidCredentials();
            }

            try
            {
                // Hash new password
                var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
                var passwordHash = new PasswordHash(hash, salt);

                // Update password
                _userManager.ChangePassword(user, passwordHash);
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

