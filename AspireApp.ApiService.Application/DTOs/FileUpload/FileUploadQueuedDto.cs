namespace AspireApp.ApiService.Application.DTOs.FileUpload;

/// <summary>
/// DTO for background file upload queued response
/// </summary>
public record FileUploadQueuedDto
{
    /// <summary>
    /// File ID that can be used to check upload status
    /// </summary>
    public Guid FileId { get; init; }

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Message indicating the file upload has been queued
    /// </summary>
    public string Message { get; init; } = "File upload has been queued and will be processed in the background. Please check the file status later.";
}

