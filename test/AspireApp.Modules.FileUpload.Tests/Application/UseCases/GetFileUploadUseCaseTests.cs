using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Application.UseCases;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.UseCases;

public class GetFileUploadUseCaseTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFileUploadRepository> _mockFileUploadRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<AutoMapper.IMapper> _mockMapper;

    public GetFileUploadUseCaseTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockFileUploadRepository = _fixture.Freeze<Mock<IFileUploadRepository>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockMapper = _fixture.Freeze<Mock<AutoMapper.IMapper>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenFileExists()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUploadUseCase>();
        var fileId = Guid.NewGuid();
        var fileUpload = _fixture.Create<AspireApp.Modules.FileUpload.Domain.Entities.FileUpload>();
        var fileDto = _fixture.Create<FileUploadDto>();

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileUpload);

        _mockMapper.Setup(x => x.Map<FileUploadDto>(fileUpload))
            .Returns(fileDto);

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(fileDto);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenFileDoesNotExist()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUploadUseCase>();
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
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var useCase = _fixture.Create<GetFileUploadUseCase>();
        var fileId = Guid.NewGuid();

        _mockFileUploadRepository.Setup(x => x.GetAsync(fileId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await useCase.ExecuteAsync(fileId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("General.InternalError");
        result.Error.Message.Should().Contain("Database error");
    }
}
