using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Application.UseCases;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.UseCases;

public class DeleteFileUseCaseTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFileUploadRepository> _mockFileUploadRepository;
    private readonly Mock<IFileStorageStrategyFactory> _mockStorageStrategyFactory;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFileStorageStrategy> _mockStorageStrategy;

    public DeleteFileUseCaseTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockFileUploadRepository = _fixture.Freeze<Mock<IFileUploadRepository>>();
        _mockStorageStrategyFactory = _fixture.Freeze<Mock<IFileStorageStrategyFactory>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockStorageStrategy = _fixture.Freeze<Mock<IFileStorageStrategy>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteFileAndEntity_WhenFound()
    {
        // Arrange
        var useCase = _fixture.Create<DeleteFileUseCase>();
        var fileId = Guid.NewGuid();
        var storagePath = "/path/to/file";
        
        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            "test.jpg", "image/jpeg", 100, ".jpg", FileType.Image, FileStorageType.FileSystem, null, null, null, null)
        {
            StoragePath = storagePath
        };

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        _mockStorageStrategyFactory.Setup(x => x.GetStrategy(FileStorageType.FileSystem))
            .Returns(_mockStorageStrategy.Object);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockStorageStrategy.Verify(x => x.DeleteAsync(storagePath, It.IsAny<CancellationToken>()), Times.Once);
        // Explicitly verify the overload that matches the call
        _mockFileUploadRepository.Verify(x => x.DeleteAsync(fileUpload, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenFileNotFound()
    {
        // Arrange
        var useCase = _fixture.Create<DeleteFileUseCase>();
        var fileId = Guid.NewGuid();

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AspireApp.Modules.FileUpload.Domain.Entities.FileUpload?)null);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("General.NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinueWhenStorageDeletionFails()
    {
        // Arrange
        var useCase = _fixture.Create<DeleteFileUseCase>();
        var fileId = Guid.NewGuid();
        var storagePath = "/path/to/file";
        
        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            "test.jpg", "image/jpeg", 100, ".jpg", FileType.Image, FileStorageType.FileSystem, null, null, null, null)
        {
            StoragePath = storagePath
        };

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        _mockStorageStrategyFactory.Setup(x => x.GetStrategy(FileStorageType.FileSystem))
            .Returns(_mockStorageStrategy.Object);

        _mockStorageStrategy.Setup(x => x.DeleteAsync(storagePath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage error"));

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockFileUploadRepository.Verify(x => x.DeleteAsync(fileUpload, It.IsAny<CancellationToken>()), Times.Once);
    }
}
