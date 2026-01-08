using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Application.Roles.UseCases;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Roles.Services;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Roles.UseCases;

public class UpdateRoleUseCaseTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleManager _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UpdateRoleUseCase _useCase;

    public UpdateRoleUseCaseTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _roleManager = Substitute.For<IRoleManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _useCase = new UpdateRoleUseCase(_roleRepository, _roleManager, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldUpdateRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest("Updated Description");
        var role = new Role("Admin", "Old Description", RoleType.Admin);
        var roleDto = new RoleDto(roleId, "Admin", "Updated Description", "Admin", new List<string>());

        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(role);
        _roleRepository.UpdateAsync(role, Arg.Any<CancellationToken>()).Returns(role);
        _mapper.Map<RoleDto>(role).Returns(roleDto);

        // Act
        var result = await _useCase.ExecuteAsync(roleId, request);

        // Assert
        Assert.True(result.IsSuccess);
        _roleManager.Received(1).UpdateDescription(role, request.Description!);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingRole_ShouldReturnNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest("Updated Description");
        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act
        var result = await _useCase.ExecuteAsync(roleId, request);

        // Assert
        Assert.False(result.IsSuccess);
    }
}

