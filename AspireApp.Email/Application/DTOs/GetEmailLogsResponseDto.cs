namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for email logs response
/// </summary>
public class GetEmailLogsResponseDto
{
    public List<EmailLogDto> EmailLogs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

