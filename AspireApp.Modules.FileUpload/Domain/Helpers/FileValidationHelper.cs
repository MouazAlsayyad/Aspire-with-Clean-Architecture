using AspireApp.Modules.FileUpload.Domain.Enums;
using System.Buffers;
using System.Security.Cryptography;

namespace AspireApp.Modules.FileUpload.Domain.Helpers;

/// <summary>
/// Helper class for file validation and utilities
/// </summary>
public static class FileValidationHelper
{
    // Maximum file sizes (in bytes)
    public const long MaxImageSize = 10 * 1024 * 1024; // 10 MB
    public const long MaxDocumentSize = 50 * 1024 * 1024; // 50 MB
    public const long MaxVideoSize = 500 * 1024 * 1024; // 500 MB
    public const long MaxAudioSize = 50 * 1024 * 1024; // 50 MB
    public const long MaxOtherSize = 100 * 1024 * 1024; // 100 MB

    // Allowed image extensions
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };

    // Allowed image content types
    public static readonly string[] AllowedImageContentTypes =
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp",
        "image/svg+xml"
    };

    /// <summary>
    /// Gets the maximum file size for a given file type
    /// </summary>
    public static long GetMaxFileSize(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => MaxImageSize,
            FileType.Document => MaxDocumentSize,
            FileType.Video => MaxVideoSize,
            FileType.Audio => MaxAudioSize,
            _ => MaxOtherSize
        };
    }

    /// <summary>
    /// Validates if the file extension is allowed for images
    /// </summary>
    public static bool IsAllowedImageExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        var normalizedExtension = extension.StartsWith('.') ? extension : "." + extension;
        return AllowedImageExtensions.Contains(normalizedExtension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if the content type is allowed for images
    /// </summary>
    public static bool IsAllowedImageContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        // Use Span<char> for zero-allocation string operations
        ReadOnlySpan<char> contentTypeSpan = contentType;
        var semicolonIndex = contentTypeSpan.IndexOf(';');
        ReadOnlySpan<char> baseContentType = semicolonIndex >= 0
            ? contentTypeSpan.Slice(0, semicolonIndex).Trim()
            : contentTypeSpan.Trim();

        // Convert to string only for comparison (unavoidable with Contains)
        var baseContentTypeStr = baseContentType.ToString();
        return AllowedImageContentTypes.Contains(baseContentTypeStr, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates file size against the maximum allowed for the file type
    /// </summary>
    public static bool IsValidFileSize(long fileSize, FileType fileType)
    {
        var maxSize = GetMaxFileSize(fileType);
        return fileSize > 0 && fileSize <= maxSize;
    }

    /// <summary>
    /// Computes MD5 hash of a stream using optimized Span&lt;byte&gt; and Memory&lt;byte&gt; operations
    /// </summary>
    public static async Task<string> ComputeHashAsync(Stream stream)
    {
        var originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            const int bufferSize = 81920; // 80KB buffer for optimal performance

            // Use ArrayPool for buffer allocation to reduce GC pressure
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(new Memory<byte>(buffer), CancellationToken.None)) > 0)
                {
                    md5.AppendData(new ReadOnlySpan<byte>(buffer, 0, bytesRead));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            var hashBytes = md5.GetHashAndReset();

            // Use string.Create with stackalloc for zero-allocation string building (MD5 is always 16 bytes = 32 hex chars)
            return string.Create(32, hashBytes, static (span, bytes) =>
            {
                const string hexChars = "0123456789abcdef";
                for (int i = 0; i < bytes.Length; i++)
                {
                    span[i * 2] = hexChars[bytes[i] >> 4];
                    span[i * 2 + 1] = hexChars[bytes[i] & 0x0F];
                }
            });
        }
        finally
        {
            stream.Position = originalPosition; // Reset position after reading
        }
    }

    /// <summary>
    /// Validates if a file is an image based on extension and content type
    /// </summary>
    public static bool IsImageFile(string? extension, string? contentType)
    {
        var isExtensionValid = !string.IsNullOrWhiteSpace(extension) && IsAllowedImageExtension(extension);
        var isContentTypeValid = !string.IsNullOrWhiteSpace(contentType) && IsAllowedImageContentType(contentType);

        return isExtensionValid || isContentTypeValid;
    }
}

