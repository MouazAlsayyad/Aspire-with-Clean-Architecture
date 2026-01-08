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

public class RefreshTokenUseCaseTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly RefreshTokenUseCase _useCase;

    public RefreshTokenUseCaseTests()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _configuration = Substitute.For<IConfiguration>();

        _configuration["Jwt:AccessTokenExpirationHours"].Returns("1");
        _configuration["Jwt:RefreshTokenExpirationDays"].Returns("7");

        _useCase = new RefreshTokenUseCase(
            _refreshTokenRepository,
            _userRepository,
            _tokenService,
            _unitOfWork,
            _mapper,
            _configuration);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRefreshToken_ShouldReturnNewAuthResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest("valid-refresh-token");
        var refreshToken = new RefreshToken(userId, request.RefreshToken, DateTime.UtcNow.AddDays(7));
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        var userDto = new UserDto(userId, user.Email, user.UserName, user.FirstName, user.LastName, true, true, "en", new List<string>());

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns(refreshToken);
        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(user);
        _tokenService.GenerateAccessToken(user).Returns(newAccessToken);
        _tokenService.GenerateRefreshToken().Returns(newRefreshToken);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(newAccessToken, result.Value.Token);
        Assert.Equal(newRefreshToken, result.Value.RefreshToken);
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingToken_ShouldReturnNotFoundError()
    {
        // Arrange
        var request = new RefreshTokenRequest("non-existing-token");
        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        _refreshTokenRepository.GetByTokenIncludeRevokedAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRevokedToken_ShouldReturnReusedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest("revoked-token");
        var revokedToken = new RefreshToken(userId, request.RefreshToken, DateTime.UtcNow.AddDays(7));
        revokedToken.Revoke();

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        _refreshTokenRepository.GetByTokenIncludeRevokedAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns(revokedToken);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("reused", result.Error.Message);
        await _refreshTokenRepository.Received(1).RevokeAllUserTokensAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveUser_ShouldReturnAccountDeactivatedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest("valid-token");
        var refreshToken = new RefreshToken(userId, request.RefreshToken, DateTime.UtcNow.AddDays(7));
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        user.Deactivate();

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns(refreshToken);
        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("deactivated", result.Error.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRevokeOldToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RefreshTokenRequest("valid-token");
        var refreshToken = new RefreshToken(userId, request.RefreshToken, DateTime.UtcNow.AddDays(7));
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>()).Returns(refreshToken);
        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(user);
        _tokenService.GenerateAccessToken(user).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns("new-refresh-token");
        _mapper.Map<UserDto>(user).Returns(new UserDto(userId, user.Email, user.UserName, user.FirstName, user.LastName, true, true, "en", new List<string>()));

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        await _refreshTokenRepository.Received(1).UpdateAsync(Arg.Is<RefreshToken>(rt => rt.IsRevoked), Arg.Any<CancellationToken>());
    }
}

