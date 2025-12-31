using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Helpers;
using System.Buffers;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Domain service (Manager) for FileUpload entity.
/// Handles file upload-related domain logic and business rules.
/// Throws DomainException for business rule violations.
/// </summary>
public class FileUploadManager : DomainService, IFileUploadManager
{
    // Buffer size constant for stream operations
    private const int BufferSize = 81920; // 80KB buffer for optimal performance

    /// <inheritdoc />
    public async Task<(string Extension, FileType FileType, string? Hash)> ValidateAndProcessFileAsync(
        string fileName,
        string contentType,
        long fileSize,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (fileSize <= 0)
        {
            throw new DomainException(DomainErrors.General.InvalidInput("File size must be greater than zero."));
        }

        // Get and validate file extension
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new DomainException(DomainErrors.General.InvalidInput("File must have an extension."));
        }

        // Determine file type using FileTypeHelper
        var fileType = FileTypeHelper.GetFileType(extension, contentType);

        // Validate file size for the file type using FileValidationHelper
        if (!FileValidationHelper.IsValidFileSize(fileSize, fileType))
        {
            var maxSize = FileValidationHelper.GetMaxFileSize(fileType);
            throw new DomainException(
                DomainErrors.General.InvalidInput($"File size exceeds maximum allowed size of {maxSize / (1024 * 1024)} MB for {fileType.ToString()} files."));
        }

        // For images, validate extension and content type using FileValidationHelper
        if (fileType == FileType.Image)
        {
            if (!FileValidationHelper.IsImageFile(extension, contentType))
            {
                throw new DomainException(
                    DomainErrors.General.InvalidInput("Invalid image file. Only JPG, JPEG, PNG, GIF, BMP, WEBP, and SVG are allowed."));
            }
        }

        // Compute file hash for duplicate detection using FileValidationHelper
        string? fileHash = null;
        try
        {
            fileHash = await FileValidationHelper.ComputeHashAsync(fileStream);
        }
        catch
        {
            // Hash computation failure is not critical, continue without hash
        }

        return (extension, fileType, fileHash);
    }

    /// <inheritdoc />
    public FileUpload CreateFileUpload(
        string fileName,
        string contentType,
        long fileSize,
        string extension,
        FileType fileType,
        FileStorageType storageType,
        Guid? uploadedBy = null,
        string? description = null,
        string? tags = null,
        string? hash = null,
        string? storagePath = null,
        byte[]? fileContent = null)
    {
        var fileUpload = new FileUpload(
            fileName: fileName,
            contentType: contentType,
            fileSize: fileSize,
            extension: extension,
            fileType: fileType,
            storageType: storageType,
            uploadedBy: uploadedBy,
            description: description,
            tags: tags,
            hash: hash)
        {
            StoragePath = storagePath,
            FileContent = fileContent
        };

        return fileUpload;
    }

    /// <inheritdoc />
    public async Task<byte[]> ReadFileIntoMemoryAsync(
        Stream fileStream,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        fileStream.Position = 0;
        using var memoryStream = new MemoryStream((int)fileSize);

        await CopyStreamWithBufferAsync(fileStream, memoryStream, cancellationToken);

        return memoryStream.ToArray();
    }

    /// <inheritdoc />
    public async Task<string?> ComputeFileHashAsync(
        byte[] fileBytes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var hashStream = new MemoryStream(fileBytes);
            return await FileValidationHelper.ComputeHashAsync(hashStream);
        }
        catch
        {
            return null;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Copies stream content using ArrayPool buffer for optimal performance.
    /// </summary>
    private static async Task CopyStreamWithBufferAsync(
        Stream sourceStream,
        Stream destinationStream,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await sourceStream.ReadAsync(new Memory<byte>(buffer), cancellationToken)) > 0)
            {
                await destinationStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #endregion
}

