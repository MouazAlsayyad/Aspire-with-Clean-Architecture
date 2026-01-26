using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Helpers;
using FluentAssertions;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Domain.Helpers;

public class FileValidationHelperTests
{
    [Theory]
    [InlineData(FileType.Image, 10 * 1024 * 1024)]
    [InlineData(FileType.Document, 50 * 1024 * 1024)]
    [InlineData(FileType.Video, 500 * 1024 * 1024)]
    [InlineData(FileType.Audio, 50 * 1024 * 1024)]
    [InlineData(FileType.Other, 100 * 1024 * 1024)]
    public void GetMaxFileSize_ShouldReturnCorrectSize(FileType fileType, long expectedSize)
    {
        // Act
        var result = FileValidationHelper.GetMaxFileSize(fileType);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(".jpg", true)]
    [InlineData("png", true)]
    [InlineData(".pdf", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAllowedImageExtension_ShouldValidateCorrectly(string? extension, bool expectedResult)
    {
        // Act
        var result = FileValidationHelper.IsAllowedImageExtension(extension!);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("application/pdf", false)]
    [InlineData("image/jpeg; charset=utf-8", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAllowedImageContentType_ShouldValidateCorrectly(string? contentType, bool expectedResult)
    {
        // Act
        var result = FileValidationHelper.IsAllowedImageContentType(contentType!);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(100, FileType.Image, true)]
    [InlineData(10 * 1024 * 1024 + 1, FileType.Image, false)]
    [InlineData(0, FileType.Image, false)]
    [InlineData(50 * 1024 * 1024, FileType.Document, true)]
    public void IsValidFileSize_ShouldValidateCorrectly(long fileSize, FileType fileType, bool expectedResult)
    {
        // Act
        var result = FileValidationHelper.IsValidFileSize(fileSize, fileType);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ComputeHashAsync_ShouldReturnCorrectHash()
    {
        // Arrange
        var content = "Hello World";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        
        // Expected hash for "Hello World"
        // echo -n "Hello World" | md5sum
        // b10a8db164e0754105b7a99be72e3fe5
        var expectedHash = "b10a8db164e0754105b7a99be72e3fe5";

        // Act
        var result = await FileValidationHelper.ComputeHashAsync(stream);

        // Assert
        result.Should().Be(expectedHash);
        stream.Position.Should().Be(0); // Should reset stream position
    }

    [Theory]
    [InlineData(".jpg", "image/jpeg", true)]
    [InlineData(".jpg", null, true)]
    [InlineData(null, "image/jpeg", true)]
    [InlineData(".pdf", "application/pdf", false)]
    [InlineData(".jpg", "application/pdf", true)] // One valid is enough
    [InlineData(".pdf", "image/jpeg", true)] // One valid is enough
    public void IsImageFile_ShouldValidateCorrectly(string? extension, string? contentType, bool expectedResult)
    {
        // Act
        var result = FileValidationHelper.IsImageFile(extension, contentType);

        // Assert
        result.Should().Be(expectedResult);
    }
}
