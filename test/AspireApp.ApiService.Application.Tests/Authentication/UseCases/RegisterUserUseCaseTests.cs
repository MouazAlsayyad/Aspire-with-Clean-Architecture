using AspireApp.ApiService.Application.Authentication.DTOs;
using AspireApp.ApiService.Application.Authentication.UseCases;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Authentication.UseCases;

public class RegisterUserUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserManager _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _userManager = Substitute.For<IUserManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _useCase = new RegisterUserUseCase(
            _userRepository,
            _roleRepository,
            _passwordHasher,
            _userManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldRegisterUser()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123!", "First", "Last");
        var hash = "hash";
        var salt = "salt";
        var user = new User(request.Email, request.UserName, PasswordHash.Create(hash, salt), request.FirstName, request.LastName);
        var defaultRole = new Role("User", "User Role", RoleType.User);
        var userDto = new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, false, true, "en", new List<string> { "User" });

        _passwordHasher.HashPassword(request.Password).Returns((hash, salt));
        _userManager.CreateAsync(request.Email, request.UserName, Arg.Any<PasswordHash>(), request.FirstName, request.LastName, Arg.Any<CancellationToken>())
            .Returns(user);
        _roleRepository.GetByNameAsync("User", Arg.Any<CancellationToken>()).Returns(defaultRole);
        _userRepository.InsertAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userDto.Email, result.Value.Email);
        await _userManager.Received(1).AssignRoleAsync(user, defaultRole.Id, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutDefaultRole_ShouldStillRegisterUser()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123!", "First", "Last");
        var hash = "hash";
        var salt = "salt";
        var user = new User(request.Email, request.UserName, PasswordHash.Create(hash, salt), request.FirstName, request.LastName);
        var userDto = new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, false, true, "en", new List<string>());

        _passwordHasher.HashPassword(request.Password).Returns((hash, salt));
        _userManager.CreateAsync(request.Email, request.UserName, Arg.Any<PasswordHash>(), request.FirstName, request.LastName, Arg.Any<CancellationToken>())
            .Returns(user);
        _roleRepository.GetByNameAsync("User", Arg.Any<CancellationToken>()).Returns((Role?)null);
        _userRepository.InsertAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.DidNotReceive().AssignRoleAsync(Arg.Any<User>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHashPassword()
    {
        // Arrange
        var request = new RegisterRequest("test@test.com", "testuser", "Password123!", "First", "Last");
        var user = new User(request.Email, request.UserName, PasswordHash.Create("hash", "salt"), request.FirstName, request.LastName);

        _passwordHasher.HashPassword(request.Password).Returns(("hash", "salt"));
        _userManager.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<PasswordHash>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _userRepository.InsertAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UserDto>(user).Returns(new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, false, true, "en", new List<string>()));

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _passwordHasher.Received(1).HashPassword(request.Password);
    }
}

