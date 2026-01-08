using AspireApp.ApiService.Application.Permissions.DTOs;
using AspireApp.ApiService.Application.Permissions.UseCases;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Permissions.UseCases;

public class GetAllPermissionsUseCaseTests
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GetAllPermissionsUseCase _useCase;

    public GetAllPermissionsUseCaseTests()
    {
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _useCase = new GetAllPermissionsUseCase(_permissionRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllPermissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission("User.Read", "Read User", "User", "Read"),
            new Permission("User.Write", "Write User", "User", "Write")
        };
        var permissionDtos = new List<PermissionDto>
        {
            new PermissionDto(Guid.NewGuid(), "User.Read", "Read User", "User", "Read", "User.Read"),
            new PermissionDto(Guid.NewGuid(), "User.Write", "Write User", "User", "Write", "User.Write")
        };

        _permissionRepository.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(permissions);
        _mapper.Map<IEnumerable<PermissionDto>>(permissions).Returns(permissionDtos);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }
}

