using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Auth;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.ApiService.Application.UseCases.Authentication;

/// <summary>
/// Professional refresh token use case implementation following security best practices:
/// - Token rotation (old token revoked, new token issued)
/// - Reuse detection (if revoked token is reused, revoke all user tokens)
/// - Transaction handling for atomicity
/// - Comprehensive validation and error handling
/// </summary>
public class RefreshTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Start transaction for atomicity
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // First, try to get a valid (non-revoked, non-expired) token
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            
            // If not found as valid token, check if it exists but is revoked (reuse detection)
            if (refreshToken == null)
            {
                var revokedToken = await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(request.RefreshToken, cancellationToken);
                
                if (revokedToken != null)
                {
                    // Token exists but is revoked - this indicates token reuse (potential theft)
                    // Security best practice: Revoke ALL tokens for this user
                    if (!revokedToken.IsExpired)
                    {
                        await _refreshTokenRepository.RevokeAllUserTokensAsync(revokedToken.UserId, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    }
                    
                    return DomainErrors.RefreshToken.Reused();
                }
                
                // Token doesn't exist at all
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return DomainErrors.RefreshToken.NotFound();
            }

            // Token is valid, proceed with refresh
            // Get the user with all necessary data
            var user = await _userRepository.GetAsync(refreshToken.UserId, cancellationToken: cancellationToken);
            if (user == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return DomainErrors.User.NotFound(refreshToken.UserId);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return DomainErrors.User.AccountDeactivated();
            }

            // Token rotation: Revoke the old refresh token
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Create and save new refresh token
            var newRefreshTokenEntity = new Domain.Entities.RefreshToken(
                user.Id,
                newRefreshToken,
                DateTime.UtcNow.AddDays(7)); // Refresh token expires in 7 days

            await _refreshTokenRepository.InsertAsync(newRefreshTokenEntity, cancellationToken);

            // Commit transaction atomically
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Map user to DTO
            var userDto = _mapper.Map<UserDto>(user);

            var authResponse = new AuthResponse(newAccessToken, newRefreshToken, expiresAt, userDto);
            return Result.Success(authResponse);
        }
        catch (Exception)
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

