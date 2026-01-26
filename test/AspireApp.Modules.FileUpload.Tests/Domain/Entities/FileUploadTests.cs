using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Domain.Entities;

public class FileUploadTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var extension = ".jpg";
        var fileType = FileType.Image;
        var storageType = FileStorageType.FileSystem;
        var uploadedBy = Guid.NewGuid();
        var description = "Test description";
        var tags = "test,image";
        var hash = "hash123";

        // Act
        var fileUpload = new AspireApp.Modules.FileUpload.Domain.Entities.FileUpload(
            fileName,
            contentType,
            fileSize,
            extension,
            fileType,
            storageType,
            uploadedBy,
            description,
            tags,
            hash);

        // Assert
        fileUpload.FileName.Should().Be(fileName);
        fileUpload.ContentType.Should().Be(contentType);
        fileUpload.FileSize.Should().Be(fileSize);
        fileUpload.Extension.Should().Be(extension);
        fileUpload.FileType.Should().Be(fileType);
        fileUpload.StorageType.Should().Be(storageType);
        fileUpload.UploadedBy.Should().Be(uploadedBy);
        fileUpload.Description.Should().Be(description);
        fileUpload.Tags.Should().Be(tags);
        fileUpload.Hash.Should().Be(hash);
    }
}
