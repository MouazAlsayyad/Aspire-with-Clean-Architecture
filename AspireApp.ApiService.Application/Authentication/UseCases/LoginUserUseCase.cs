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

public class LoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    // Token expiration values
    private readonly int accessTokenExpirationHours;
    private readonly int refreshTokenExpirationDays;
    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;

        // Get expiration values from configuration
        accessTokenExpirationHours = int.Parse(_configuration["Jwt:AccessTokenExpirationHours"] ?? "1");
        refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return DomainErrors.User.InvalidCredentials();
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return DomainErrors.User.AccountDeactivated();
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Hash, user.PasswordHash.Salt))
        {
            return DomainErrors.User.InvalidCredentials();
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expiresAt = DateTime.UtcNow.AddHours(accessTokenExpirationHours);

        // Create and save refresh token entity
        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(refreshTokenExpirationDays));

        await _refreshTokenRepository.InsertAsync(refreshTokenEntity, cancellationToken);

        // Save changes to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(user);

        var authResponse = new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
        return Result.Success(authResponse);
    }
}
