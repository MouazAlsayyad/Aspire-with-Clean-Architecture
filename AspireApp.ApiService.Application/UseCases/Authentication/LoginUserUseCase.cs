using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Auth;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.ApiService.Application.UseCases.Authentication;

public class LoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Create and save refresh token entity
        var refreshTokenEntity = new Domain.Entities.RefreshToken(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)); // Refresh token expires in 7 days

        await _refreshTokenRepository.InsertAsync(refreshTokenEntity, cancellationToken);
        
        // Save changes to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(user);

        var authResponse = new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
        return Result.Success(authResponse);
    }
}
