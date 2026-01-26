using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Helpers;
using FluentAssertions;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Domain.Helpers;

public class FileTypeHelperTests
{
    [Theory]
    [InlineData(".jpg", FileType.Image)]
    [InlineData(".png", FileType.Image)]
    [InlineData(".pdf", FileType.Document)]
    [InlineData(".mp4", FileType.Video)]
    [InlineData(".mp3", FileType.Audio)]
    [InlineData(".unknown", FileType.Other)]
    [InlineData("", FileType.Other)]
    [InlineData(null, FileType.Other)]
    public void GetFileTypeFromExtension_ShouldReturnCorrectType(string? extension, FileType expectedType)
    {
        // Act
        var result = FileTypeHelper.GetFileTypeFromExtension(extension!);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void GetFileTypeFromExtension_ShouldHandleExtensionWithoutDot()
    {
        // Act
        var result = FileTypeHelper.GetFileTypeFromExtension("jpg");

        // Assert
        result.Should().Be(FileType.Image);
    }

    [Theory]
    [InlineData("image/jpeg", FileType.Image)]
    [InlineData("application/pdf", FileType.Document)]
    [InlineData("video/mp4", FileType.Video)]
    [InlineData("audio/mpeg", FileType.Audio)]
    [InlineData("application/unknown", FileType.Other)]
    [InlineData("", FileType.Other)]
    [InlineData(null, FileType.Other)]
    public void GetFileTypeFromContentType_ShouldReturnCorrectType(string? contentType, FileType expectedType)
    {
        // Act
        var result = FileTypeHelper.GetFileTypeFromContentType(contentType!);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void GetFileTypeFromContentType_ShouldHandleContentTypeWithParameters()
    {
        // Act
        var result = FileTypeHelper.GetFileTypeFromContentType("text/plain; charset=utf-8");

        // Assert
        result.Should().Be(FileType.Document);
    }

    [Theory]
    [InlineData(".jpg", "image/jpeg", FileType.Image)]
    [InlineData(null, "image/jpeg", FileType.Image)]
    [InlineData(".jpg", null, FileType.Image)]
    [InlineData(".unknown", "image/jpeg", FileType.Image)] // Content type priority
    [InlineData(".jpg", "application/unknown", FileType.Image)] // Extension fallback
    public void GetFileType_ShouldPrioritizeContentType(string? extension, string? contentType, FileType expectedType)
    {
        // Act
        var result = FileTypeHelper.GetFileType(extension, contentType);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void IsAllowedFileType_ShouldReturnTrue_WhenTypeIsAllowed()
    {
        // Arrange
        var allowedTypes = new[] { FileType.Image, FileType.Document };

        // Act
        var result = FileTypeHelper.IsAllowedFileType(FileType.Image, allowedTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllowedFileType_ShouldReturnFalse_WhenTypeIsNotAllowed()
    {
        // Arrange
        var allowedTypes = new[] { FileType.Image, FileType.Document };

        // Act
        var result = FileTypeHelper.IsAllowedFileType(FileType.Video, allowedTypes);

        // Assert
        result.Should().BeFalse();
    }
}
