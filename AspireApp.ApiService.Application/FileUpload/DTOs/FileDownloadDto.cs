namespace AspireApp.ApiService.Application.FileUpload.DTOs;

/// <summary>
/// DTO for file download response
/// </summary>
public record FileDownloadDto
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

