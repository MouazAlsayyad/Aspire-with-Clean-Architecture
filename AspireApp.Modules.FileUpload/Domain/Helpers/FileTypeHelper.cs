using AspireApp.Modules.FileUpload.Domain.Enums;

namespace AspireApp.Modules.FileUpload.Domain.Helpers;

/// <summary>
/// Helper class for determining file types based on file extensions and content types
/// </summary>
public static class FileTypeHelper
{
    private static readonly Dictionary<string, FileType> ExtensionToFileType = new(StringComparer.OrdinalIgnoreCase)
    {
        // Image types
        { ".jpg", FileType.Image },
        { ".jpeg", FileType.Image },
        { ".png", FileType.Image },
        { ".gif", FileType.Image },
        { ".bmp", FileType.Image },
        { ".webp", FileType.Image },
        { ".svg", FileType.Image },
        { ".ico", FileType.Image },
        { ".tiff", FileType.Image },
        { ".tif", FileType.Image },

        // Document types
        { ".pdf", FileType.Document },
        { ".doc", FileType.Document },
        { ".docx", FileType.Document },
        { ".xls", FileType.Document },
        { ".xlsx", FileType.Document },
        { ".ppt", FileType.Document },
        { ".pptx", FileType.Document },
        { ".txt", FileType.Document },
        { ".rtf", FileType.Document },
        { ".odt", FileType.Document },
        { ".ods", FileType.Document },
        { ".odp", FileType.Document },

        // Video types
        { ".mp4", FileType.Video },
        { ".avi", FileType.Video },
        { ".mov", FileType.Video },
        { ".wmv", FileType.Video },
        { ".flv", FileType.Video },
        { ".webm", FileType.Video },
        { ".mkv", FileType.Video },
        { ".m4v", FileType.Video },

        // Audio types
        { ".mp3", FileType.Audio },
        { ".wav", FileType.Audio },
        { ".ogg", FileType.Audio },
        { ".wma", FileType.Audio },
        { ".flac", FileType.Audio },
        { ".aac", FileType.Audio },
        { ".m4a", FileType.Audio }
    };

    private static readonly Dictionary<string, FileType> ContentTypeToFileType = new(StringComparer.OrdinalIgnoreCase)
    {
        // Image content types
        { "image/jpeg", FileType.Image },
        { "image/jpg", FileType.Image },
        { "image/png", FileType.Image },
        { "image/gif", FileType.Image },
        { "image/bmp", FileType.Image },
        { "image/webp", FileType.Image },
        { "image/svg+xml", FileType.Image },
        { "image/x-icon", FileType.Image },
        { "image/tiff", FileType.Image },

        // Document content types
        { "application/pdf", FileType.Document },
        { "application/msword", FileType.Document },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileType.Document },
        { "application/vnd.ms-excel", FileType.Document },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileType.Document },
        { "application/vnd.ms-powerpoint", FileType.Document },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", FileType.Document },
        { "text/plain", FileType.Document },
        { "application/rtf", FileType.Document },
        { "application/vnd.oasis.opendocument.text", FileType.Document },
        { "application/vnd.oasis.opendocument.spreadsheet", FileType.Document },
        { "application/vnd.oasis.opendocument.presentation", FileType.Document },

        // Video content types
        { "video/mp4", FileType.Video },
        { "video/x-msvideo", FileType.Video },
        { "video/quicktime", FileType.Video },
        { "video/x-ms-wmv", FileType.Video },
        { "video/x-flv", FileType.Video },
        { "video/webm", FileType.Video },
        { "video/x-matroska", FileType.Video },

        // Audio content types
        { "audio/mpeg", FileType.Audio },
        { "audio/wav", FileType.Audio },
        { "audio/ogg", FileType.Audio },
        { "audio/x-ms-wma", FileType.Audio },
        { "audio/flac", FileType.Audio },
        { "audio/aac", FileType.Audio },
        { "audio/mp4", FileType.Audio }
    };

    /// <summary>
    /// Determines the file type based on file extension
    /// </summary>
    public static FileType GetFileTypeFromExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return FileType.Other;

        // Ensure extension starts with a dot
        if (!extension.StartsWith('.'))
            extension = "." + extension;

        return ExtensionToFileType.TryGetValue(extension, out var fileType)
            ? fileType
            : FileType.Other;
    }

    /// <summary>
    /// Determines the file type based on content type (MIME type) using Span&lt;char&gt; for zero-allocation parsing
    /// </summary>
    public static FileType GetFileTypeFromContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return FileType.Other;

        // Use Span<char> for zero-allocation string operations
        ReadOnlySpan<char> contentTypeSpan = contentType;
        var semicolonIndex = contentTypeSpan.IndexOf(';');
        ReadOnlySpan<char> baseContentType = semicolonIndex >= 0
            ? contentTypeSpan.Slice(0, semicolonIndex).Trim()
            : contentTypeSpan.Trim();

        // Convert to string only for dictionary lookup (unavoidable)
        var baseContentTypeStr = baseContentType.ToString();
        return ContentTypeToFileType.TryGetValue(baseContentTypeStr, out var fileType)
            ? fileType
            : FileType.Other;
    }

    /// <summary>
    /// Determines the file type based on both extension and content type
    /// Prioritizes content type if available, otherwise falls back to extension
    /// </summary>
    public static FileType GetFileType(string? extension, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var typeFromContentType = GetFileTypeFromContentType(contentType);
            if (typeFromContentType != FileType.Other)
                return typeFromContentType;
        }

        if (!string.IsNullOrWhiteSpace(extension))
        {
            return GetFileTypeFromExtension(extension);
        }

        return FileType.Other;
    }

    /// <summary>
    /// Validates if the file type is allowed (e.g., only images)
    /// </summary>
    public static bool IsAllowedFileType(FileType fileType, FileType[] allowedTypes)
    {
        return allowedTypes.Contains(fileType);
    }
}

