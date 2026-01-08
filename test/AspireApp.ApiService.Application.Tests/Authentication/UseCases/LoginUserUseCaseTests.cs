using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Authentication.UseCases;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Authentication.Entities;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Authentication.UseCases;

public class LoginUserUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly LoginUserUseCase _useCase;

    public LoginUserUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _configuration = Substitute.For<IConfiguration>();

        _configuration["Jwt:AccessTokenExpirationHours"].Returns("1");
        _configuration["Jwt:RefreshTokenExpirationDays"].Returns("7");

        _useCase = new LoginUserUseCase(
            _userRepository,
            _passwordHasher,
            _tokenService,
            _refreshTokenRepository,
            _unitOfWork,
            _mapper,
            _configuration);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var userDto = new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, true, true, "en", new List<string>());

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Hash, user.PasswordHash.Salt).Returns(true);
        _tokenService.GenerateAccessToken(user).Returns(accessToken);
        _tokenService.GenerateRefreshToken().Returns(refreshToken);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(accessToken, result.Value.Token);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingUser_ShouldReturnInvalidCredentialsError()
    {
        // Arrange
        var request = new LoginRequest("nonexisting@test.com", "password");
        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveUser_ShouldReturnAccountDeactivatedError()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        user.Deactivate();

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("deactivated", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ShouldReturnInvalidCredentialsError()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "wrongpassword");
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Hash, user.PasswordHash.Salt).Returns(false);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateRefreshToken()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Hash, user.PasswordHash.Salt).Returns(true);
        _tokenService.GenerateAccessToken(user).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");
        _mapper.Map<UserDto>(user).Returns(new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, true, true, "en", new List<string>()));

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        await _refreshTokenRepository.Received(1).InsertAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}

