using AspireApp.Domain.Shared.Common;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Services;
using FluentAssertions;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Domain.Services;

public class FileUploadManagerTests
{
    private readonly FileUploadManager _manager;

    public FileUploadManagerTests()
    {
        _manager = new FileUploadManager();
    }

    [Fact]
    public async Task ValidateAndProcessFileAsync_ShouldReturnCorrectData_WhenFileIsValid()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileContent = new byte[] { 1, 2, 3 };
        using var fileStream = new MemoryStream(fileContent);
        var fileSize = fileContent.Length;

        // Act
        var result = await _manager.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream);

        // Assert
        result.Extension.Should().Be(".jpg");
        result.FileType.Should().Be(FileType.Image);
        result.Hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateAndProcessFileAsync_ShouldThrowException_WhenFileSizeIsZero()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        using var fileStream = new MemoryStream();
        var fileSize = 0L;

        // Act
        Func<Task> act = async () => await _manager.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Code == "General.InvalidInput");
    }

    [Fact]
    public async Task ValidateAndProcessFileAsync_ShouldThrowException_WhenExtensionIsMissing()
    {
        // Arrange
        var fileName = "test";
        var contentType = "image/jpeg";
        var fileContent = new byte[] { 1, 2, 3 };
        using var fileStream = new MemoryStream(fileContent);
        var fileSize = fileContent.Length;

        // Act
        Func<Task> act = async () => await _manager.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Code == "General.InvalidInput");
    }

    [Fact]
    public async Task ValidateAndProcessFileAsync_ShouldThrowException_WhenFileSizeExceedsLimit()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 10 * 1024 * 1024 + 1; // Exceeds 10MB limit
        using var fileStream = new MemoryStream(); // Stream content doesn't matter for size check in this mock scenario, but logic checks fileSize param

        // Act
        Func<Task> act = async () => await _manager.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Code == "General.InvalidInput");
    }

    [Fact]
    public async Task ValidateAndProcessFileAsync_ShouldThrowException_WhenImageFileIsInvalid()
    {
        // Arrange
        var fileName = "test.tiff"; // .tiff is recognized as Image type but not in AllowedImageExtensions
        var contentType = "application/octet-stream"; // Unknown content type
        var fileContent = new byte[] { 1, 2, 3 };
        using var fileStream = new MemoryStream(fileContent);
        var fileSize = fileContent.Length;

        // Act
        Func<Task> act = async () => await _manager.ValidateAndProcessFileAsync(fileName, contentType, fileSize, fileStream);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Code == "General.InvalidInput");
    }

    [Fact]
    public void CreateFileUpload_ShouldReturnEntity_WithCorrectProperties()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var extension = ".jpg";
        var fileType = FileType.Image;
        var storageType = FileStorageType.FileSystem;
        var uploadedBy = Guid.NewGuid();
        var description = "Test";
        var tags = "tag";
        var hash = "hash";
        var storagePath = "path";
        var fileContent = new byte[] { 1 };

        // Act
        var result = _manager.CreateFileUpload(fileName, contentType, fileSize, extension, fileType, storageType, uploadedBy, description, tags, hash, storagePath, fileContent);

        // Assert
        result.FileName.Should().Be(fileName);
        result.ContentType.Should().Be(contentType);
        result.StoragePath.Should().Be(storagePath);
        result.FileContent.Should().BeEquivalentTo(fileContent);
    }

    [Fact]
    public async Task ReadFileIntoMemoryAsync_ShouldReturnByteArray()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        using var fileStream = new MemoryStream(fileContent);
        var fileSize = fileContent.Length;

        // Act
        var result = await _manager.ReadFileIntoMemoryAsync(fileStream, fileSize);

        // Assert
        result.Should().BeEquivalentTo(fileContent);
    }
}
