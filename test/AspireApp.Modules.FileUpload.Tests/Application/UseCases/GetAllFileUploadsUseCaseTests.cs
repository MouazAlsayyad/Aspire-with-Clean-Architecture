using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Application.UseCases;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.UseCases;

public class GetAllFileUploadsUseCaseTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFileUploadRepository> _mockFileUploadRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<AutoMapper.IMapper> _mockMapper;

    public GetAllFileUploadsUseCaseTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockFileUploadRepository = _fixture.Freeze<Mock<IFileUploadRepository>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockMapper = _fixture.Freeze<Mock<AutoMapper.IMapper>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnListOfFileUploads_WhenFilesExist()
    {
        // Arrange
        var useCase = _fixture.Create<GetAllFileUploadsUseCase>();
        var fileUploads = _fixture.CreateMany<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>(3).ToList();
        var fileDtos = _fixture.CreateMany<FileUploadDto>(3).ToList();

        _mockFileUploadRepository.Setup(x => x.GetListAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUploads);

        _mockMapper.Setup(x => x.Map<List<FileUploadDto>>(fileUploads))
            .Returns(fileDtos);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(fileDtos);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoFilesExist()
    {
        // Arrange
        var useCase = _fixture.Create<GetAllFileUploadsUseCase>();
        var fileUploads = new List<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>();
        var fileDtos = new List<FileUploadDto>();

        _mockFileUploadRepository.Setup(x => x.GetListAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUploads);

        _mockMapper.Setup(x => x.Map<List<FileUploadDto>>(fileUploads))
            .Returns(fileDtos);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var useCase = _fixture.Create<GetAllFileUploadsUseCase>();

        _mockFileUploadRepository.Setup(x => x.GetListAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("General.InternalError");
        result.Error.Message.Should().Contain("Database error");
    }
}
