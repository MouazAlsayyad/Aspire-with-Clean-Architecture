using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.UseCases;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Users.UseCases;

public class UserUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserManager _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _userManager = Substitute.For<IUserManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
    }

    [Fact]
    public async Task UpdateUserUseCase_ShouldUpdateUserAndSave()
    {
        // Arrange
        var useCase = new UpdateUserUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("NewFirst", "NewLast", true);
        var user = CreateTestUser(userId);

        _userRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(x => Task.FromResult((User)x[0]));
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(x => Task.FromResult((User)x[0]));

        // Act
        var result = await useCase.ExecuteAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserUseCase_ShouldDeleteUserAndSave()
    {
        // Arrange
        var useCase = new DeleteUserUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        _userRepository.DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _userRepository.DeleteAsync(Arg.Any<User>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserUseCase_ShouldReturnUserDto()
    {
        // Arrange
        var useCase = new GetUserUseCase(_userRepository, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        var userDto = CreateTestUserDto(userId);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userDto, result.Value);
    }

    [Fact]
    public async Task ToggleUserActivationUseCase_ShouldToggleAndSave()
    {
        // Arrange
        var useCase = new ToggleUserActivationUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var request = new ToggleUserActivationRequest(false);
        var user = CreateTestUser(userId);
        _userRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        // Act
        var result = await useCase.ExecuteAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        _userManager.Received(1).Deactivate(Arg.Any<User>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRoleToUserUseCase_ShouldAssignAndSave()
    {
        // Arrange
        var useCase = new AssignRoleToUserUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var roleIds = new List<Guid> { roleId };
        var request = new AssignRoleToUserRequest(roleIds);

        var user = CreateTestUser(userId);
        _userRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        // Act
        var result = await useCase.ExecuteAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.Received(1).SetRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignPermissionsToUserUseCase_ShouldAssignAndSave()
    {
        // Arrange
        var useCase = new AssignPermissionsToUserUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var permissionIds = new List<Guid> { Guid.NewGuid() };
        var request = new AssignPermissionsToUserRequest(permissionIds);

        var user = CreateTestUser(userId);
        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        // Act
        var result = await useCase.ExecuteAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.Received(1).SetPermissionsAsync(user, permissionIds, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdatePasswordUseCase_ShouldUpdateAndSave()
    {
        // Arrange
        var useCase = new UpdatePasswordUseCase(_userRepository, _passwordHasher, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var request = new UpdatePasswordRequest("CurrentPass", "NewPass");
        var user = CreateTestUser(userId);

        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));
        _passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash.Hash, user.PasswordHash.Salt).Returns(true);
        _passwordHasher.HashPassword(request.NewPassword).Returns(("newHash", "newSalt"));

        // Act
        var result = await useCase.ExecuteAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        _userManager.Received(1).ChangePassword(user, Arg.Any<PasswordHash>());
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveRoleFromUserUseCase_ShouldRemoveAndSave()
    {
        // Arrange
        var useCase = new RemoveRoleFromUserUseCase(_userRepository, _userManager, _unitOfWork, _mapper);
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser(userId);

        _userRepository.GetAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((User?)user));

        // Act
        var result = await useCase.ExecuteAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _userManager.Received(1).RemoveRole(user, roleId);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private User CreateTestUser(Guid id)
    {
        var user = new User("test@test.com", "user", PasswordHash.Create("h", "s"), "F", "L");
        return user;
    }

    private UserDto CreateTestUserDto(Guid id)
    {
        return new UserDto(id, "test@test.com", "user", "F", "L", true, true, "en", new List<string>());
    }
}
