using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Application.Roles.UseCases;
using AspireApp.ApiService.Application.Roles.Validators;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Roles.Services;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Roles;

public class RoleModuleTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleModuleTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _roleManager = Substitute.For<IRoleManager>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
    }

    [Fact]
    public async Task CreateRoleUseCase_ShouldCreateAndSave()
    {
        // Arrange
        var useCase = new CreateRoleUseCase(_roleRepository, _roleManager, _unitOfWork, _mapper);
        var request = new CreateRoleRequest("NewRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager, new List<Guid>());

        var role = new Role(request.Name, request.Description, request.Type);

        _roleManager.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AspireApp.ApiService.Domain.Roles.Enums.RoleType>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(role));

        _roleRepository.InsertAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult((Role)x[0]));

        _roleRepository.InsertAsync(Arg.Any<Role>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult((Role)x[0]));

        _roleRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((Role?)role));

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRoleRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        _roleManager.RoleNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var validator = new CreateRoleRequestValidator(_roleManager);
        var request = new CreateRoleRequest("UniqueRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager, new List<Guid>());

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task GetRoleUseCase_ShouldReturnRoleDto()
    {
        // Arrange
        var useCase = new GetRoleUseCase(_roleRepository, _unitOfWork, _mapper);
        var roleId = Guid.NewGuid();
        var role = new Role("TestRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager);
        var roleDto = new RoleDto(roleId, "TestRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager.ToString(), new List<string>());

        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((Role?)role));
        _mapper.Map<RoleDto>(role).Returns(roleDto);

        // Act
        var result = await useCase.ExecuteAsync(roleId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(roleDto, result.Value);
    }

    [Fact]
    public async Task DeleteRoleUseCase_ShouldDeleteAndSave()
    {
        // Arrange
        var useCase = new DeleteRoleUseCase(_roleRepository, _roleManager, _unitOfWork, _mapper);
        var roleId = Guid.NewGuid();
        var role = new Role("TestRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager);

        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((Role?)role));

        // Act
        var result = await useCase.ExecuteAsync(roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _roleManager.Received(1).ValidateDeletion(role);
        await _roleRepository.Received(1).DeleteAsync(role, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemovePermissionFromRoleUseCase_ShouldRemoveAndSave()
    {
        // Arrange
        var useCase = new RemovePermissionFromRoleUseCase(_roleRepository, _roleManager, _unitOfWork, _mapper);
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var role = new Role("TestRole", "Desc", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager);

        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((Role?)role));

        // Act
        var result = await useCase.ExecuteAsync(roleId, permissionId);

        // Assert
        Assert.True(result.IsSuccess);
        _roleManager.Received(1).RemovePermission(role, permissionId);
        await _roleRepository.Received(1).UpdateAsync(role, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
