using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.UseCases;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Users.UseCases;

public class CreateUserUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserManager _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _userManager = Substitute.For<IUserManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _useCase = new CreateUserUseCase(
            _userRepository,
            _roleRepository,
            _passwordHasher,
            _userManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Execute_WithValidRequest_ShouldCreateUserAndReturnSuccess()
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "user", "Pass123!", "First", "Last");
        var salt = "salt";
        var hash = "hash";
        _passwordHasher.HashPassword(request.Password).Returns((hash, salt));

        var user = new User(request.Email, request.UserName, PasswordHash.Create(hash, salt), request.FirstName, request.LastName);
        _userManager.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<PasswordHash>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var role = new Role("User", "User Role", AspireApp.ApiService.Domain.Roles.Enums.RoleType.User);
        _roleRepository.GetByNameAsync("User", Arg.Any<CancellationToken>()).Returns(role);

        _userRepository.InsertAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        var userDto = new UserDto(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.IsEmailConfirmed, user.IsActive, user.Language, new List<string> { "User" });
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userDto, result.Value);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
