using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Authentication.Entities;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AspireApp.ApiService.Application.Authentication.UseCases;

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
    private readonly IConfiguration _configuration;

    // Token expiration values
    private readonly int accessTokenExpirationHours;
    private readonly int refreshTokenExpirationDays;
    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;

        // Get expiration values from configuration
        accessTokenExpirationHours = int.Parse(_configuration["Jwt:AccessTokenExpirationHours"] ?? "1");
        refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
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

            var expiresAt = DateTime.UtcNow.AddHours(accessTokenExpirationHours);

            // Create and save new refresh token
            var newRefreshTokenEntity = new RefreshToken(
                user.Id,
                newRefreshToken,
                DateTime.UtcNow.AddDays(refreshTokenExpirationDays));

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

