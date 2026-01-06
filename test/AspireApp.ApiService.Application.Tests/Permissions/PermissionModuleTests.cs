using AspireApp.ApiService.Application.Permissions.DTOs;
using AspireApp.ApiService.Application.Permissions.UseCases;
using AspireApp.ApiService.Application.Permissions.Validators;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Services;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Permissions;

public class PermissionModuleTests
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionManager _permissionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PermissionModuleTests()
    {
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _permissionManager = Substitute.For<IPermissionManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
    }

    [Fact]
    public async Task CreatePermissionUseCase_ShouldCreateAndSave()
    {
        // Arrange
        var useCase = new CreatePermissionUseCase(_permissionRepository, _permissionManager, _unitOfWork, _mapper);
        var request = new CreatePermissionRequest("User.Delete", "Delete User", "User", "Delete");

        var permission = new Permission(request.Name, request.Description, request.Resource, request.Action);
        _permissionManager.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(permission);
        _permissionRepository.InsertAsync(permission, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePermissionRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        _permissionManager.PermissionNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var validator = new CreatePermissionRequestValidator(_permissionManager);
        var request = new CreatePermissionRequest("Unique.Perm", "Desc", "Res", "Act");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
