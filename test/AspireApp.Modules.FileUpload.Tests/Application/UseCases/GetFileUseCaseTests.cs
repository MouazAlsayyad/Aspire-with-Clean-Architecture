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

public class GetFileUseCaseTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFileUploadRepository> _mockFileUploadRepository;
    private readonly Mock<IFileStorageStrategyFactory> _mockStorageStrategyFactory;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFileStorageStrategy> _mockStorageStrategy;

    public GetFileUseCaseTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockFileUploadRepository = _fixture.Freeze<Mock<IFileUploadRepository>>();
        _mockStorageStrategyFactory = _fixture.Freeze<Mock<IFileStorageStrategyFactory>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockStorageStrategy = _fixture.Freeze<Mock<IFileStorageStrategy>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFile_WhenFoundAndStorageIsDatabase()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUseCase>();
        var fileId = Guid.NewGuid();
        var fileContent = new byte[] { 1, 2, 3 };
        
        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            "test.jpg", "image/jpeg", fileContent.Length, ".jpg", FileType.Image, FileStorageType.Database, null, null, null, null)
        {
            FileContent = fileContent
        };
        // Reflection to set Id if needed, or assume mock returns it with Id match
        // BaseEntity usually has protected set Id or similar.
        // But repository returns this object, so it's fine.

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
        result.Value.FileStream.Should().NotBeNull();
        
        using var memoryStream = new MemoryStream();
        await result.Value.FileStream.CopyToAsync(memoryStream);
        memoryStream.ToArray().Should().BeEquivalentTo(fileContent);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFile_WhenFoundAndStorageIsFileSystem()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUseCase>();
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

        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        _mockStorageStrategy.Setup(x => x.DownloadAsync(storagePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenFileNotFound()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUseCase>();
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
    public async Task ExecuteAsync_ShouldReturnFailure_WhenFileContentMissingInDatabase()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUseCase>();
        var fileId = Guid.NewGuid();
        
        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            "test.jpg", "image/jpeg", 100, ".jpg", FileType.Image, FileStorageType.Database, null, null, null, null)
        {
            FileContent = null
        };

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("General.NotFound");
    }
}
