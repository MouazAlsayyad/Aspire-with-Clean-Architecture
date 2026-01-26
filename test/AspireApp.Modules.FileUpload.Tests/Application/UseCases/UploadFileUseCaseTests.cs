using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Application.UseCases;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.UseCases;

public class UploadFileUseCaseTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFileUploadRepository> _mockFileUploadRepository;
    private readonly Mock<IFileStorageStrategyFactory> _mockStorageStrategyFactory;
    private readonly Mock<IFileUploadManager> _mockFileUploadManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFileStorageStrategy> _mockStorageStrategy;

    public UploadFileUseCaseTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockFileUploadRepository = _fixture.Freeze<Mock<IFileUploadRepository>>();
        _mockStorageStrategyFactory = _fixture.Freeze<Mock<IFileStorageStrategyFactory>>();
        _mockFileUploadManager = _fixture.Freeze<Mock<IFileUploadManager>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockStorageStrategy = _fixture.Freeze<Mock<IFileStorageStrategy>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUploadFileAndSaveToDatabase_WhenNotUsingBackgroundQueue()
    {
        // Arrange
        var useCase = _fixture.Create<UploadFileUseCase>();
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileContent = new byte[] { 1, 2, 3 };
        using var fileStream = new MemoryStream(fileContent);
        var fileSize = fileContent.Length;
        var request = new UploadFileRequest
        {
            StorageType = FileStorageType.FileSystem,
            Description = "Test file",
            Tags = "test",
            UseBackgroundQueue = false
        };
        var uploadedBy = Guid.NewGuid();
        var storagePath = "/uploads/test.jpg";
        var extension = ".jpg";
        var fileType = FileType.Image;
        var fileHash = "hash123";

        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            fileName, contentType, fileSize, extension, fileType, request.StorageType, uploadedBy, request.Description, request.Tags, fileHash);
        
        var fileUploadDto = new FileUploadDto();

        _mockFileUploadManager.Setup(x => x.ValidateAndProcessFileAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((extension, fileType, fileHash));

        _mockStorageStrategyFactory.Setup(x => x.GetStrategy(request.StorageType))
            .Returns(_mockStorageStrategy.Object);

        _mockStorageStrategy.Setup(x => x.UploadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storagePath);

        _mockFileUploadManager.Setup(x => x.CreateFileUpload(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), 
            It.IsAny<FileType>(), It.IsAny<FileStorageType>(), It.IsAny<Guid?>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(fileUpload);

        _mockFileUploadRepository.Setup(x => x.InsertAsync(It.IsAny<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        _mockMapper.Setup(x => x.Map<FileUploadDto>(It.IsAny<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>()))
            .Returns(fileUploadDto);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(fileName, contentType, fileStream, fileSize, request, uploadedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(fileUploadDto);

        _mockFileUploadManager.Verify(x => x.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream, It.IsAny<CancellationToken>()), Times.Once);
        _mockStorageStrategyFactory.Verify(x => x.GetStrategy(request.StorageType), Times.Once);
        _mockStorageStrategy.Verify(x => x.UploadAsync(fileName, contentType, fileStream, It.IsAny<CancellationToken>()), Times.Once);
        _mockFileUploadRepository.Verify(x => x.InsertAsync(It.IsAny<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var useCase = _fixture.Create<UploadFileUseCase>();
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        using var fileStream = new MemoryStream();
        var fileSize = 0L;
        var request = new UploadFileRequest();
        
        _mockFileUploadManager.Setup(x => x.ValidateAndProcessFileAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException("Invalid file"));

        // Act
        var result = await useCase.ExecuteAsync(fileName, contentType, fileStream, fileSize, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("General.InvalidInput");
    }
}
